using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/listening")]
public class ListeningController : ControllerBase
{
    private readonly IListeningService _listening;
    private readonly IAnonymousVisitorService _visitors;
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    public ListeningController(IListeningService listening, IAnonymousVisitorService visitors,
        AppDbContext db, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _listening = listening;
        _visitors = visitors;
        _db = db;
        _environment = environment;
        _configuration = configuration;
    }
    private async Task<long?> VisitorIdAsync()
    {
        Request.Cookies.TryGetValue("tm_visitor", out string? token);
        var visitor = await _visitors.FindExistingAsync(token);
        return visitor?.Id;
    }
    public sealed record StartRequest(long ShopId, double? Latitude = null, double? Longitude = null,
        double? AccuracyMeters = null, DateTimeOffset? CapturedAt = null, int? AudioId = null);
    public sealed record PositionRequest(double? Latitude = null, double? Longitude = null,
        double? AccuracyMeters = null, DateTimeOffset? CapturedAt = null, long? PlayedSeconds = null);

    private async Task<bool> LocationAllowedAsync(long shopId, double? latitude,
        double? longitude, double? accuracyMeters, DateTimeOffset? capturedAt)
    {
        var shop = await _db.Shops.AsNoTracking()
            .Where(x => x.Id == shopId)
            .Select(x => new { x.Latitude, x.Longitude, x.ActivationRadiusMeters })
            .FirstOrDefaultAsync();

        return shop is not null && ThuyetMinh.Api.Services.GeoFencePolicy.IsAllowed(
            shop.Latitude, shop.Longitude, shop.ActivationRadiusMeters,
            latitude, longitude, accuracyMeters, capturedAt, DateTimeOffset.UtcNow);
    }

    [HttpPost("start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start([FromBody] StartRequest request)
    {
        var id = await VisitorIdAsync();
        if (id == null) return Unauthorized();
        // Approved places only in production. Draft samples are allowed for localhost demo only.
        bool localDemo = _environment.IsDevelopment() && ThuyetMinh.Api.Services.DemoNetwork.IsDirectLocalRequest(HttpContext);
        bool shopAllowed = await _db.Shops.AnyAsync(x => x.Id == request.ShopId &&
            (x.PublishStatus == PublishStatus.Approved || localDemo));
        if (!shopAllowed) return NotFound(new { message = "Địa điểm không có nội dung đã duyệt." });
        if ((_configuration.GetValue<bool>("Listening:RequireGps", true) || !_environment.IsDevelopment()) && !localDemo &&
            !await LocationAllowedAsync(request.ShopId, request.Latitude, request.Longitude,
                request.AccuracyMeters, request.CapturedAt))
            return StatusCode(403, new { message = "Vị trí GPS không hợp lệ hoặc ngoài bán kính địa điểm." });

        if (request.AudioId is not > 0 ||
            ResolveAudioFile(await FindPublishedAudioAsync(request.ShopId, request.AudioId.Value)) is null)
            return NotFound(new { message = "Audio không thuộc nội dung đang được công bố." });
        var session = await _listening.StartAsync(id.Value, request.ShopId, request.AudioId);
        if (session == null) return StatusCode(402, new { message = "Đã hết thời lượng nghe." });
        return Ok(session);
    }

    [HttpPost("sessions/{sessionId:long}/{operation}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long sessionId, string operation,
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] PositionRequest? position)
    {
        if (operation is not ("heartbeat" or "pause" or "resume" or "end"))
            return NotFound();
        if (position?.PlayedSeconds is < 0 || (operation == "heartbeat" && position?.PlayedSeconds == null))
            return BadRequest(new { message = "Invalid playback duration." });
        var id = await VisitorIdAsync();
        if (id == null) return Unauthorized();
        bool localDemo = _environment.IsDevelopment() &&
            ThuyetMinh.Api.Services.DemoNetwork.IsDirectLocalRequest(HttpContext);

        if (operation is "heartbeat" or "resume")
        {
            var session = await _db.ListeningSessions.AsNoTracking()
                .Where(x => x.Id == sessionId && x.AnonymousVisitorId == id.Value)
                .Select(x => new { x.ShopId, x.AudioId, x.Status })
                .FirstOrDefaultAsync();

            if (session is null) return NotFound();
            if (session.Status != ListeningSessionStatus.Ended &&
                (session.AudioId is not > 0 ||
                 await FindPublishedAudioAsync(session.ShopId, session.AudioId.Value) is null))
            {
                await _listening.UpdateAsync(id.Value, sessionId, "end", position?.PlayedSeconds);
                return StatusCode(403, new { message = "Audio không còn được công bố." });
            }

            if (session.Status != ListeningSessionStatus.Ended &&
                (_configuration.GetValue<bool>("Listening:RequireGps", true) || !_environment.IsDevelopment()) &&
                !localDemo &&
                !await LocationAllowedAsync(session.ShopId, position?.Latitude, position?.Longitude,
                    position?.AccuracyMeters, position?.CapturedAt))
            {
                await _listening.UpdateAsync(id.Value, sessionId, "pause", position?.PlayedSeconds);
                return StatusCode(403, new {
                    message = "GPS không hợp lệ hoặc đã ra ngoài bán kính. Phiên nghe đã tạm dừng."
                });
            }
        }

        var snapshot = await _listening.UpdateAsync(id.Value, sessionId, operation, position?.PlayedSeconds);
        if (snapshot == null) return NotFound();
        return Ok(snapshot);
    }

    private Task<string?> FindPublishedAudioAsync(long shopId, int audioId) =>
        (from audio in _db.Audios
         join translation in _db.Translations on audio.TranslationId equals translation.Id
         join submission in _db.ShopSubmissions on translation.ShopSubmissionId equals submission.Id
         join shop in _db.Shops on submission.ShopId equals shop.Id
         join language in _db.Languages on translation.LanguageId equals language.Id
         where audio.Id == audioId && shop.Id == shopId &&
             audio.Status == "Success" && translation.Status == "Success" && language.IsActive &&
             submission.Status == SubmissionStatus.Approved &&
             shop.PublishStatus == PublishStatus.Approved &&
             shop.PublishedVersionId == submission.Id
         select audio.FilePath).FirstOrDefaultAsync();

    [HttpGet("sessions/{sessionId:long}/audio")]
    public async Task<IActionResult> StreamAudio(long sessionId)
    {
        var visitorId = await VisitorIdAsync();
        if (visitorId == null) return Unauthorized();

        var session = await _db.ListeningSessions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == sessionId && x.AnonymousVisitorId == visitorId);
        if (session == null) return NotFound();
        if (session.Status != ListeningSessionStatus.Playing ||
            session.LastHeartbeatAt < DateTime.UtcNow.AddSeconds(-ListeningService.MaxHeartbeatAgeSeconds) ||
            session.AudioId is not > 0)
            return StatusCode(403);
        if (!await _db.ListeningBalances.AnyAsync(x =>
            x.AnonymousVisitorId == visitorId && x.RemainingSeconds > 0))
            return StatusCode(402);

        var file = ResolveAudioFile(await FindPublishedAudioAsync(session.ShopId, session.AudioId.Value));
        if (file == null) return NotFound();
        Response.Headers.CacheControl = "no-store, private";
        return PhysicalFile(file, "audio/mpeg", enableRangeProcessing: true);
    }

    private string? ResolveAudioFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(_environment.WebRootPath))
            return null;
        var normalized = path.Replace('\\', '/').TrimStart('/');
        if (!normalized.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)) return null;
        try
        {
            var root = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "audio")) +
                Path.DirectorySeparatorChar;
            var file = Path.GetFullPath(Path.Combine(_environment.WebRootPath, normalized));
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return file.StartsWith(root, comparison) &&
                string.Equals(Path.GetExtension(file), ".mp3", StringComparison.OrdinalIgnoreCase) &&
                System.IO.File.Exists(file) ? file : null;
        }
        catch (ArgumentException) { return null; }
        catch (NotSupportedException) { return null; }
    }
}
