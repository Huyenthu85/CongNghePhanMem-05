using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Api.Controllers;

[AllowAnonymous]
[Route("images/shop-uploads")]
public class ShopImageController(AppDbContext db, IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet("{shopId:long}/{fileName}")]
    public async Task<IActionResult> Get(long shopId, string fileName, CancellationToken ct)
    {
        var url = $"/images/shop-uploads/{shopId}/{fileName}";
        if (!ShopService.IsUploadedImageUrl(url)) return NotFound();

        var admin = User.IsInRole("Admin");
        var owner = User.IsInRole("Owner") &&
            long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out _);
        long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

        var candidates = await db.ShopSubmissions.AsNoTracking()
            .Where(s => s.ShopId == shopId && s.ImageUrlsRaw.Contains(url)
                && (admin || (owner && s.Shop.OwnerId == userId)
                    || (s.Status == SubmissionStatus.Approved
                        && s.Shop.PublishStatus == PublishStatus.Approved
                        && s.Shop.PublishedVersionId == s.Id)))
            .Select(s => s.ImageUrlsRaw).ToListAsync(ct);
        if (!candidates.Any(raw => ShopService.ParseImageUrls(raw).Contains(url)))
            return NotFound();

        var path = Path.Combine(environment.ContentRootPath, "App_Data", "shop-images",
            shopId.ToString(), fileName);
        if (!System.IO.File.Exists(path)) return NotFound();
        var contentType = Path.GetExtension(fileName) switch
        {
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["Cache-Control"] = "private, no-store";
        return PhysicalFile(path, contentType);
    }
}
