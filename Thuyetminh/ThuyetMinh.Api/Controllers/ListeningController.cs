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
        double? AccuracyMeters = null, DateTimeOffset? CapturedAt = null);
    public sealed record PositionRequest(double? Latitude = null, double? Longitude = null,
        double? AccuracyMeters = null, DateTimeOffset? CapturedAt = null);

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
        if (_configuration.GetValue<bool>("Listening:RequireGps") && !localDemo &&
            !await LocationAllowedAsync(request.ShopId, request.Latitude, request.Longitude,
                request.AccuracyMeters, request.CapturedAt))
            return StatusCode(403, new { message = "Vị trí GPS không hợp lệ hoặc ngoài bán kính địa điểm." });

        var session = await _listening.StartAsync(id.Value, request.ShopId);
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
        var id = await VisitorIdAsync();
        if (id == null) return Unauthorized();
        bool localDemo = _environment.IsDevelopment() &&
            ThuyetMinh.Api.Services.DemoNetwork.IsDirectLocalRequest(HttpContext);

        if (_configuration.GetValue<bool>("Listening:RequireGps") && !localDemo &&
            (operation is "heartbeat" or "resume"))
        {
            var shopId = await _db.ListeningSessions.AsNoTracking()
                .Where(x => x.Id == sessionId && x.AnonymousVisitorId == id.Value &&
                    x.Status != ListeningSessionStatus.Ended)
                .Select(x => x.ShopId)
                .FirstOrDefaultAsync();

            if (shopId == 0) return NotFound();

            if (!await LocationAllowedAsync(shopId, position?.Latitude, position?.Longitude,
                    position?.AccuracyMeters, position?.CapturedAt))
            {
                await _listening.UpdateAsync(id.Value, sessionId, "pause");
                return StatusCode(403, new {
                    message = "GPS không hợp lệ hoặc đã ra ngoài bán kính. Phiên nghe đã tạm dừng."
                });
            }
        }

        var snapshot = await _listening.UpdateAsync(id.Value, sessionId, operation);
        if (snapshot == null) return NotFound();
        return Ok(snapshot);
    }
}
