using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Dtos;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        var token = await _auth.LoginAsync(req.Username, req.Password);
        return Ok(new LoginResponse(token));
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        _auth.Logout(token);
        return NoContent();
    }
}