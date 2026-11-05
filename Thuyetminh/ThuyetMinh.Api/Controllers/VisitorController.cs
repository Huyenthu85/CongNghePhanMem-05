
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data.Repositories;
namespace ThuyetMinh.Api.Controllers;
[ApiController]
[Route("api/visitor")]
[AllowAnonymous]
public class VisitorController : ControllerBase
{
    private const string CookieName = "tm_visitor";
    private readonly IAnonymousVisitorService _visitorService;
    private readonly IListeningBalanceRepository _balanceRepository;
    public VisitorController(IAnonymousVisitorService visitorService, IListeningBalanceRepository balanceRepository)
    {
        _visitorService = visitorService;
        _balanceRepository = balanceRepository;
    }
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        Request.Cookies.TryGetValue(CookieName, out string? token);
        var result = await _visitorService.GetOrCreateAsync(token);
        if (result.NewToken != null)
        {
            Response.Cookies.Append(CookieName, result.NewToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                IsEssential = true,
                Path = "/"
            });
        }
        var balance = await _balanceRepository.GetOrCreateAsync(result.Visitor.Id);
        return Ok(new
        {
            visitorId = result.Visitor.Id,
            remainingSeconds = balance.RemainingSeconds
        });
    }
}
