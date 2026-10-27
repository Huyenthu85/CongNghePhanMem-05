using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Api.Services;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Api.Controllers;

/// <summary>
/// Simulates a payment provider for a classroom demo. A phone scans the QR,
/// but the purchased seconds are credited to the desktop checkout visitor.
/// This is NOT a banking integration.
/// </summary>
[ApiController]
public sealed class DemoQrController : ControllerBase
{
    public sealed class IssueRequest { public string? OrderCode { get; set; } }
    public sealed class RedeemRequest { public string? Code { get; set; } }

    private readonly IWebHostEnvironment _environment;
    private readonly DemoQrTokenStore _tickets;
    private readonly IAnonymousVisitorService _visitors;
    private readonly IPaymentService _payments;
    private readonly IPaymentConfirmationService _confirmation;

    public DemoQrController(IWebHostEnvironment environment, DemoQrTokenStore tickets,
        IAnonymousVisitorService visitors, IPaymentService payments,
        IPaymentConfirmationService confirmation)
    {
        _environment = environment;
        _tickets = tickets;
        _visitors = visitors;
        _payments = payments;
        _confirmation = confirmation;
    }

    // Admin creates the QR locally, bound to one Pending order belonging to their
    // anonymous visitor cookie. Remote users cannot create arbitrary free credits.
    [HttpPost("/api/demo/qr/issue")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin")]
    // Temporary classroom-only workaround for broken antiforgery verification.
    // Do not deploy this to a public/production environment.
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Issue([FromBody] IssueRequest input)
    {
        if (!_environment.IsDevelopment() || !DemoNetwork.IsDirectLocalRequest(HttpContext))
            return NotFound();
        Request.Cookies.TryGetValue("tm_visitor", out var token);
        var visitor = await _visitors.FindExistingAsync(token);
        if (visitor == null || !visitor.IsActive) return Unauthorized();
        if (string.IsNullOrWhiteSpace(input.OrderCode)) return BadRequest(new { message = "Thiếu mã đơn." });
        var order = await _payments.GetOrderAsync(input.OrderCode);
        if (order == null || order.AnonymousVisitorId != visitor.Id) return NotFound();
        if (order.Status != PaymentStatus.Pending || order.ExpiresAt <= DateTime.UtcNow)
            return Conflict(new { message = "Đơn không ở trạng thái chờ hoặc đã hết hạn." });
        try
        {
            var ticket = _tickets.Issue(visitor.Id, order.OrderCode, new DateTimeOffset(order.ExpiresAt, TimeSpan.Zero));
            Response.Headers.CacheControl = "no-store";
            return Ok(new { ticket.Code, ticket.ExpiresAt, orderCode = order.OrderCode, demo = true });
        }
        catch (InvalidOperationException e)
        {
            return Conflict(new { message = e.Message });
        }
    }

    // GET is read-only. It returns a browser page that makes a POST on load;
    // this prevents messaging previews/camera link prefetches from completing a "payment".
    [AllowAnonymous]
    [HttpGet("/demo/qr/pay")]
    public IActionResult PayPage([FromQuery] string? code)
    {
        if (!_environment.IsDevelopment()) return NotFound();
        Response.Headers.CacheControl = "no-store";
        Response.Headers["Referrer-Policy"] = "no-referrer";
        Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
        string safeCode = JsonSerializer.Serialize(code ?? string.Empty);
        string html = $$"""
            <!doctype html><html lang="vi"><head><meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <meta name="referrer" content="no-referrer">
            <title>Thanh toán mô phỏng</title>
            <style>body{font:16px system-ui,sans-serif;max-width:460px;padding:40px 20px;margin:40px auto;color:#243b50;text-align:center}
            .card{border:1px solid #dce6e5;border-radius:18px;padding:28px;box-shadow:0 12px 30px #0000000b}
            h1{font-size:23px}p{line-height:1.7}button{padding:12px 20px;border:0;border-radius:8px;background:#264156;color:white;cursor:pointer}</style>
            </head><body><div class="card"><h1>Thanh toán mô phỏng</h1>
            <p>Gói 20.000 VNĐ – 900 giây. Không chuyển tiền thật.</p>
            <p id="result" role="status">Đã quét QR. Đang xác nhận giao dịch DEMO...</p>
            <button id="retry" style="display:none" type="button">Thử lại</button>
            </div><script>
            const code = {{safeCode}};
            async function confirmDemo() {
              const el = document.getElementById('result');
              el.textContent = 'Đang xác nhận giao dịch DEMO...';
              document.getElementById('retry').style.display = 'none';
              try {
                const response = await fetch('/api/demo/qr/redeem', {method:'POST',
                  headers:{'Content-Type':'application/json'},
                  credentials:'omit', body:JSON.stringify({code})});
                const data = await response.json().catch(() => ({}));
                if (!response.ok) throw new Error(data.message || 'HTTP ' + response.status);
                el.textContent = 'Thanh toán MÔ PHỎNG thành công! 900 giây đã được cộng vào phiên của người tạo đơn trên laptop.';
              } catch (error) {
                el.textContent = 'Không thể xác nhận: ' + error.message;
                document.getElementById('retry').style.display = 'inline-block';
              }
            }
            document.getElementById('retry').addEventListener('click',confirmDemo);
            confirmDemo();
            </script></body></html>
            """;
        return Content(html, "text/html; charset=utf-8");
    }

    // Public only because the user scans via 4G/5G. The short-lived 192-bit
    // one-order token is the DEMO authorization. No phone listening credit.
    [AllowAnonymous]
    [HttpPost("/api/demo/qr/redeem")]
    public async Task<IActionResult> Redeem([FromBody] RedeemRequest input)
    {
        if (!_environment.IsDevelopment()) return NotFound();
        Response.Headers.CacheControl = "no-store";
        var claim = _tickets.Begin(input.Code);
        if (claim.State == DemoQrTokenStore.RedeemState.Invalid)
            return BadRequest(new { message = "QR không hợp lệ hoặc đã hết hạn." });
        if (claim.State == DemoQrTokenStore.RedeemState.Busy)
            return Conflict(new { message = "Giao dịch đang xử lý. Hãy thử lại." });
        if (claim.State == DemoQrTokenStore.RedeemState.AlreadyCompleted)
            return Ok(new { status = "Paid", demo = true, alreadyConfirmed = true });
        bool success = false;
        try
        {
            var order = await _payments.GetOrderAsync(claim.OrderCode!);
            if (order == null || order.AnonymousVisitorId != claim.VisitorId)
                return BadRequest(new { message = "Đơn không còn hợp lệ." });
            success = await _confirmation.ConfirmAsync(order.OrderCode,
                $"DEMO-QR-PAY-{order.Id}", order.AmountVnd);
            if (!success)
                return Conflict(new { message = "Đơn đã hết hạn hoặc không thể xác nhận." });
            return Ok(new { status = "Paid", demo = true, grantedSeconds = order.GrantedSeconds });
        }
        finally
        {
            _tickets.Finish(input.Code, success);
        }
    }
}
