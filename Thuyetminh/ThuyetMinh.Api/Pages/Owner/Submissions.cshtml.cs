using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Pages.Owner;

[Authorize(Roles = "Owner")]
public class SubmissionsModel : PageModel
{
    private readonly IShopService _shops;
    public SubmissionsModel(IShopService shops) => _shops = shops;

    public string ShopName { get; set; } = "";
    public List<ShopSubmission> Submissions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var shop = await _shops.GetMyShopAsync(id, userId);
            ShopName = shop.Name;
            Submissions = await _shops.ListMySubmissionsAsync(id, userId);
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToPage("/Owner/ShopList");
        }
    }
}