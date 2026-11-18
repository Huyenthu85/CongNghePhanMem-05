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
    private readonly IWebHostEnvironment _environment;

    public ShopFormModel(IShopService shops, IShopRepository shopRepo, IWebHostEnvironment environment)
    {
        _shops = shops;
        _shopRepo = shopRepo;
        _environment = environment;
    }

    [BindProperty] public long ShopId { get; set; }
    [BindProperty] public ShopFormInput Input { get; set; } = new();
    [BindProperty] public List<IFormFile> ImageFiles { get; set; } = new();
    [BindProperty] public bool ReplaceImages { get; set; }
    [BindProperty] public List<string> RemovedImages { get; set; } = new();
    public string[] ExistingImages { get; private set; } = Array.Empty<string>();
    public string? Message { get; set; }
    public bool IsNew => ShopId == 0;

    public async Task<IActionResult> OnGetAsync(long? id)
    {
        Message = TempData["ShopFormMessage"] as string;
        if (id == null || id == 0)
        {
            return Page(); // Trả về form trống để tạo mới
        }

        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var shop = await _shops.GetMyShopAsync(id.Value, userId);
            ShopId = shop.Id;
            var latest = (await _shops.ListMySubmissionsAsync(shop.Id, userId)).FirstOrDefault();
            ExistingImages = ShopService.ParseImageUrls(latest?.ImageUrlsRaw);

            Input = new ShopFormInput
            {
                Name = latest?.Name ?? shop.Name,
                Address = latest?.Address ?? shop.Address,
                Latitude = latest?.Latitude ?? shop.Latitude,
                Longitude = latest?.Longitude ?? shop.Longitude,
                ActivationRadiusMeters = latest?.ActivationRadiusMeters ?? shop.ActivationRadiusMeters,
                Description = latest?.Description
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

        try
        {
            if (!IsNew)
            {
                var latest = (await _shops.ListMySubmissionsAsync(ShopId, userId)).FirstOrDefault();
                ExistingImages = ShopService.ParseImageUrls(latest?.ImageUrlsRaw);
            }
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToPage("/Owner/ShopList");
        }

        if (!ModelState.IsValid || action is not ("submit" or "draft"))
        {
            Message = "Vui lòng kiểm tra dữ liệu đã nhập.";
            return Page();
        }
        if (action == "submit" && string.IsNullOrWhiteSpace(Input.Description))
        {
            Message = "Vui lòng nhập nội dung thuyết minh trước khi gửi duyệt.";
            return Page();
        }

        var req = new ShopSubmissionRequest
        {
            Name = Input.Name,
            Address = Input.Address,
            Latitude = Input.Latitude,
            Longitude = Input.Longitude,
            ActivationRadiusMeters = Input.ActivationRadiusMeters,
            Description = Input.Description,
            ImageUrlsRaw = ReplaceImages ? "" : string.Join("\n", ExistingImages.Where(x => !RemovedImages.Contains(x))),
            Submit = action == "submit"
        };

        try
        {
            ShopService.ValidateSubmission(req);
            var images = ShopService.ParseImageUrls(req.ImageUrlsRaw).ToList();
            if (images.Count + ImageFiles.Count > 10)
                throw new BusinessException("Mỗi quán được lưu tối đa 10 ảnh.");
            if (ImageFiles.Any(f => f.Length < 12 || f.Length > 5L * 1024 * 1024)
                || ImageFiles.Sum(f => f.Length) > 20L * 1024 * 1024)
                throw new BusinessException("Ảnh phải có dung lượng tối đa 5 MB/ảnh và 20 MB/lượt tải.");
            var extensions = new List<string>();
            foreach (var file in ImageFiles)
                extensions.Add(await GetImageExtensionAsync(file, HttpContext.RequestAborted));

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
                ModelState.Remove(nameof(ShopId));
            }

            if (ImageFiles.Count > 0)
            {
                var folder = Path.Combine(
                    _environment.ContentRootPath,
                    "App_Data", "shop-images", ShopId.ToString());

                Directory.CreateDirectory(folder);

                for (int i = 0; i < ImageFiles.Count; i++)
                {
                    var name = Guid.NewGuid().ToString("N") + extensions[i];

                    await using var output = new FileStream(
                        Path.Combine(folder, name), FileMode.CreateNew);

                    await ImageFiles[i].CopyToAsync(
                        output, HttpContext.RequestAborted);

                    images.Add($"/images/shop-uploads/{ShopId}/{name}");
                }
            }

            req.ImageUrlsRaw = string.Join("\n", images);

            await _shops.CreateSubmissionAsync(ShopId, userId, req);

            TempData["ShopFormMessage"] = action == "submit"
                ? "Đã gửi duyệt. Vui lòng chờ quản trị viên phản hồi."
                : "Đã lưu nháp.";

            return RedirectToPage(new { id = ShopId });
        }
        catch (BusinessException ex)
        {
            Message = ex.Message;
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToPage("/Owner/ShopList");
        }
        catch (IOException)
        {
            Message = "Không thể lưu ảnh. Vui lòng chọn ảnh và thử lại.";
            return Page();
        }
    }

    private static async Task<string> GetImageExtensionAsync(IFormFile file, CancellationToken ct)
    {
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        await stream.ReadExactlyAsync(header, ct);
        if (header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff)
            return ".jpg";
        if (header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            return ".png";
        if (header.AsSpan(0, 4).SequenceEqual("RIFF"u8)
            && header.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return ".webp";
        throw new BusinessException("Chỉ chấp nhận ảnh JPG, PNG hoặc WEBP hợp lệ.");
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
}
