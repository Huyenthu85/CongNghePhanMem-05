using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Controllers;

// DEMO ONLY: a simulated provider callback performed by a local signed-in administrator.
[ApiController]
[Route("api/payment/demo")]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin")]
public class DemoPaymentController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly IPaymentService _payments;
    private readonly IPaymentConfirmationService _confirmation;
    private readonly IAnonymousVisitorService _visitors;

    public DemoPaymentController(IWebHostEnvironment environment, IPaymentService payments,
        IPaymentConfirmationService confirmation, IAnonymousVisitorService visitors)
    {
        _environment = environment;
        _payments = payments;
        _confirmation = confirmation;
        _visitors = visitors;
    }

    [HttpPost("confirm/{orderCode}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(string orderCode)
    {
        // Both conditions are essential. Never use this endpoint for real bank transfers.
        if (!_environment.IsDevelopment() ||
            !ThuyetMinh.Api.Services.DemoNetwork.IsDirectLocalRequest(HttpContext))
            return NotFound();

        Request.Cookies.TryGetValue("tm_visitor", out string? cookie);
        var visitor = await _visitors.FindExistingAsync(cookie);
        if (visitor == null || !visitor.IsActive) return Unauthorized();

        var order = await _payments.GetOrderAsync(orderCode);
        if (order == null || order.AnonymousVisitorId != visitor.Id) return NotFound();

        // Stable unique demo transaction per order; retry stays idempotent.
        bool confirmed = await _confirmation.ConfirmAsync(
            order.OrderCode, $"DEMO-LOCAL-{order.Id}", order.AmountVnd);
        if (!confirmed) return Conflict(new { message = "Đơn không hợp lệ, hết hạn hoặc đã được xử lý." });
        return Ok(new { status = "Paid", demo = true, grantedSeconds = order.GrantedSeconds });
    }
}
