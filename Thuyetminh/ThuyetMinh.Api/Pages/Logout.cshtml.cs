using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Pages;

public class LogoutModel(OnlineUserService onlineUsers) : PageModel
{
    public async Task<IActionResult> OnPostAsync()
    {
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) is { } id)
            onlineUsers.Untrack($"account-{id}");
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToPage("/Login");
    }
}
