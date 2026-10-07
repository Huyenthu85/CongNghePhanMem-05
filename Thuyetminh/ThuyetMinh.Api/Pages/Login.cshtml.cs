using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Pages;

public class LoginModel : PageModel
{
    private readonly IAuthService _auth;
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;

    public LoginModel(IAuthService auth, IUserRepository users, ITokenService tokens)
    {
        _auth = auth;
        _users = users;
        _tokens = tokens;
    }

    [BindProperty] public string Username { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Xác thực qua AuthService (ném AuthException nếu sai)
            await _auth.LoginAsync(Username, Password);

            var user = await _users.FindByUsernameAsync(Username);
            if (user is null)
            {
                Error = "Không tìm thấy người dùng";
                return Page();
            }

            // Tạo cookie đăng nhập
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Cookies");
            await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(identity));

            // Điều hướng theo vai trò
            if (user.Role == Role.Admin)
                return RedirectToPage("/Admin/ReviewList");
            return RedirectToPage("/Owner/ShopList");
        }
        catch (AuthException ex)
        {
            Error = ex.Message;
            return Page();
        }
    }
}