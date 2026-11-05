
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data.Models;
namespace ThuyetMinh.Api.Controllers;
[ApiController]
[Route("api/payment")]
[AllowAnonymous]
public class PaymentController : ControllerBase
{
    private const string CookieName = "tm_visitor";
    private readonly IPaymentService _paymentService;
    private readonly IAnonymousVisitorService _visitorService;
    public PaymentController(IPaymentService paymentService, IAnonymousVisitorService visitorService)
    {
        _paymentService = paymentService;
        _visitorService = visitorService;
    }
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder()
    {
        var visitor = await GetCurrentVisitorAsync();
        if (visitor == null)
        {
            return Unauthorized(new { message = "Vui long truy cap /api/visitor/me truoc." });
        }
        var order = await _paymentService.CreateOrderAsync(visitor.Id);
        return CreatedAtAction(nameof(GetOrder), new { orderCode = order.OrderCode }, ToResponse(order));
    }
    [HttpGet("orders/{orderCode}")]
    public async Task<IActionResult> GetOrder(string orderCode)
    {
        var visitor = await GetCurrentVisitorAsync();
        if (visitor == null)
        {
            return Unauthorized();
        }
        var order = await _paymentService.GetOrderAsync(orderCode);
        if (order == null || order.AnonymousVisitorId != visitor.Id)
        {
            return NotFound();
        }
        return Ok(ToResponse(order));
    }
    private async Task<AnonymousVisitor?> GetCurrentVisitorAsync()
    {
        if (!Request.Cookies.TryGetValue(CookieName, out string? token))
        {
            return null;
        }
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }
        return await _visitorService.FindExistingAsync(token);
    }
    private static object ToResponse(PaymentOrder order)
    {
        return new
        {
            orderCode = order.OrderCode,
            amountVnd = order.AmountVnd,
            grantedSeconds = order.GrantedSeconds,
            // Stored SQLite DateTime values lose Kind; all payment timestamps are UTC.
            // Compute an effective expiry without mutating the persisted order.
            status = order.Status == PaymentStatus.Pending && order.ExpiresAt <= DateTime.UtcNow
                ? PaymentStatus.Expired.ToString()
                : order.Status.ToString(),
            expiresAt = DateTime.SpecifyKind(order.ExpiresAt, DateTimeKind.Utc)
        };
    }
}
