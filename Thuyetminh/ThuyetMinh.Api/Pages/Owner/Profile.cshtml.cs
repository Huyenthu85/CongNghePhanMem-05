using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Pages.Owner;

[Authorize(Roles = "Owner")]
public class ProfileModel : PageModel
{
    private readonly IUserRepository _users;
    private readonly IShopService _shops;

    public ProfileModel(IUserRepository users, IShopService shops)
    {
        _users = users;
        _shops = shops;
    }

    public User CurrentUser { get; set; } = null!;
    public List<Shop> MyShops { get; set; } = new();
    public Dictionary<long, ShopSubmission> LatestSubmissions { get; } = new();

    public SubmissionStatus GetReviewStatus(Shop shop) =>
        LatestSubmissions.TryGetValue(shop.Id, out var latest)
            ? latest.Status
            : shop.PublishStatus switch
            {
                PublishStatus.Approved => SubmissionStatus.Approved,
                PublishStatus.Pending => SubmissionStatus.Pending,
                PublishStatus.Rejected => SubmissionStatus.Rejected,
                _ => SubmissionStatus.Draft
            };

    public async Task OnGetAsync()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        CurrentUser = await _users.FindByIdAsync(userId) ?? new User();
        MyShops = await _shops.ListMyShopsAsync(userId);

        foreach (var shop in MyShops)
        {
            var latest = (await _shops.ListMySubmissionsAsync(shop.Id, userId))
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            if (latest != null)
                LatestSubmissions[shop.Id] = latest;
        }
    }
}
