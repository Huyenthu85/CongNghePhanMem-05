using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ThuyetMinh.Business.Dtos;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Pages.Owner;

[Authorize(Roles = "Owner")]
public class ShopFormModel : PageModel
{
    private readonly IShopService _shops;
    public ShopFormModel(IShopService shops) => _shops = shops;

    [BindProperty] public long ShopId { get; set; }
    [BindProperty] public ShopFormInput Input { get; set; } = new();
    public string? Message { get; set; }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var shop = await _shops.GetMyShopAsync(id, userId);
            ShopId = shop.Id;
            Input = new ShopFormInput
            {
                Name = shop.Name,
                Address = shop.Address,
                Latitude = shop.Latitude,
                Longitude = shop.Longitude,
                ActivationRadiusMeters = shop.ActivationRadiusMeters
            };
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToPage("/Owner/ShopList");
        }
    }

    public async Task<IActionResult> OnPostAsync(string action)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var req = new ShopSubmissionRequest
        {
            Name = Input.Name,
            Address = Input.Address,
            Latitude = Input.Latitude,
            Longitude = Input.Longitude,
            ActivationRadiusMeters = Input.ActivationRadiusMeters,
            Description = Input.Description,
            ImageUrlsRaw = Input.ImageUrlsText ?? "",
            Submit = action == "submit"
        };

        try
        {
            await _shops.CreateSubmissionAsync(ShopId, userId, req);
            Message = action == "submit"
                ? "Đã gửi duyệt. Vui lòng chờ quản trị viên phản hồi."
                : "Đã lưu nháp.";
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToPage("/Owner/ShopList");
        }
    }
}

public class ShopFormInput
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double ActivationRadiusMeters { get; set; }
    public string? Description { get; set; }
    public string? ImageUrlsText { get; set; }
}