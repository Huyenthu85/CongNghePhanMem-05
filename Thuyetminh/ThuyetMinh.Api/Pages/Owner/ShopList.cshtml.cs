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

    public Dictionary<long, ShopSubmission> LatestSubmissions { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        Shops = await _shops.ListMyShopsAsync(userId);
        
        // Kiểm tra xem có quán nào đang ở trạng thái Bị từ chối không
        foreach (var shop in Shops)
        {
            var submissions = await _shops.ListMySubmissionsAsync(shop.Id, userId);

            var latest = submissions
                .OrderByDescending(s => s.CreatedAt)
                .ThenByDescending(s => s.Id)
                .FirstOrDefault();

            if (latest != null)
                LatestSubmissions[shop.Id] = latest;
        }

        HasRejectedSubmissions = LatestSubmissions.Values
            .Any(s => s.Status == SubmissionStatus.Rejected);
    }
}