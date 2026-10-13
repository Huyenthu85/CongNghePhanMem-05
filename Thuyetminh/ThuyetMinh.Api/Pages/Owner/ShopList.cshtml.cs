using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Pages.Owner;

[Authorize(Roles = "Owner")]
public class ShopListModel : PageModel
{
    private readonly IShopService _shops;
    public ShopListModel(IShopService shops) => _shops = shops;

    public List<Shop> Shops { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        Shops = await _shops.ListMyShopsAsync(userId);
    }
}
