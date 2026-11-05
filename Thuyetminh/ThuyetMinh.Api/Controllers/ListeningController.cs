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
    public ListeningController(IListeningService listening, IAnonymousVisitorService visitors,
        AppDbContext db, IWebHostEnvironment environment)
    {
        _listening = listening;
        _visitors = visitors;
        _db = db;
        _environment = environment;
    }
    private async Task<long?> VisitorIdAsync()
    {
        Request.Cookies.TryGetValue("tm_visitor", out string? token);
        var visitor = await _visitors.FindExistingAsync(token);
        return visitor?.Id;
    }
    public sealed record StartRequest(long ShopId);

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
        var session = await _listening.StartAsync(id.Value, request.ShopId);
        if (session == null) return StatusCode(402, new { message = "Đã hết thời lượng nghe." });
        return Ok(session);
    }

    [HttpPost("sessions/{sessionId:long}/{operation}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long sessionId, string operation)
    {
        if (operation is not ("heartbeat" or "pause" or "resume" or "end"))
            return NotFound();
        var id = await VisitorIdAsync();
        if (id == null) return Unauthorized();
        var snapshot = await _listening.UpdateAsync(id.Value, sessionId, operation);
        if (snapshot == null) return NotFound();
        return Ok(snapshot);
    }
}
