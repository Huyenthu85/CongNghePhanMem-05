using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Business.Services;

namespace CongNghePhanMem.Api.Pages.Owner;

[Authorize(Roles = "Owner")]
public class ShopListModel : PageModel
{
    private readonly IShopService _shops;
    public ShopListModel(IShopService shops) => _shops = shops;

    public List<Shop> Shops { get; set; } = new();
    public bool HasRejectedSubmissions { get; set; } = false;

    public async Task OnGetAsync()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        Shops = await _shops.ListMyShopsAsync(userId);
        
        // Kiểm tra xem có quán nào đang ở trạng thái Bị từ chối không
        HasRejectedSubmissions = Shops.Any(s => s.PublishStatus == PublishStatus.Rejected);
    }
}