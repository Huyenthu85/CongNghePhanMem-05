using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ThuyetMinh.Business.Dtos;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data.Repositories;

namespace CongNghePhanMem.Api.Pages.Owner;

[Authorize(Roles = "Owner")]
public class ShopFormModel : PageModel
{
    private readonly IShopService _shops;
    private readonly IShopRepository _shopRepo;

    public ShopFormModel(IShopService shops, IShopRepository shopRepo)
    {
        _shops = shops;
        _shopRepo = shopRepo;
    }

    [BindProperty] public long ShopId { get; set; }
    [BindProperty] public ShopFormInput Input { get; set; } = new();
    public string? Message { get; set; }
    public bool IsNew => ShopId == 0;

    public async Task<IActionResult> OnGetAsync(long? id)
    {
        if (id == null || id == 0)
        {
            return Page(); // Trả về form trống để tạo mới
        }

        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var shop = await _shops.GetMyShopAsync(id.Value, userId);
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
            if (IsNew)
            {
                // Nếu là tạo mới, ta tạo Shop trước (chưa public)
                var shop = new ThuyetMinh.Data.Models.Shop
                {
                    OwnerId = userId,
                    Name = req.Name,
                    Address = req.Address,
                    Latitude = req.Latitude,
                    Longitude = req.Longitude,
                    ActivationRadiusMeters = req.ActivationRadiusMeters,
                    PublishStatus = ThuyetMinh.Data.Models.PublishStatus.Draft
                };
                await _shopRepo.AddAsync(shop);
                ShopId = shop.Id;
            }
            
            // Sau đó tạo Submission cho quán đó
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