using ThuyetMinh.Business.Dtos;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;
using System.ComponentModel.DataAnnotations;

namespace ThuyetMinh.Business.Services;

public interface IShopService
{
    Task<List<Shop>> ListMyShopsAsync(long ownerId);
    Task<Shop> GetMyShopAsync(long shopId, long ownerId);
    Task<ShopSubmission> CreateSubmissionAsync(long shopId, long ownerId, ShopSubmissionRequest req);
    Task<List<ShopSubmission>> ListMySubmissionsAsync(long shopId, long ownerId);
    Task<ShopContentResponse?> GetPublishedContentAsync(long shopId);
}

public class ShopService : IShopService
{
    private readonly IShopRepository _shops;
    private readonly IShopSubmissionRepository _subs;

    public ShopService(IShopRepository shops, IShopSubmissionRepository subs)
    {
        _shops = shops;
        _subs = subs;
    }

    public Task<List<Shop>> ListMyShopsAsync(long ownerId) =>
        _shops.FindByOwnerIdAsync(ownerId);

    public async Task<Shop> GetMyShopAsync(long shopId, long ownerId)
    {
        var shop = await _shops.FindByIdAndOwnerIdAsync(shopId, ownerId);
        return shop ?? throw new UnauthorizedAccessException("Không có quyền truy cập quán này");
    }

    public async Task<ShopSubmission> CreateSubmissionAsync(
        long shopId, long ownerId, ShopSubmissionRequest req)
    {
        var shop = await GetMyShopAsync(shopId, ownerId);
        ValidateSubmission(req);

        var sub = new ShopSubmission
        {
            ShopId = shop.Id,
            SubmittedById = ownerId,
            Name = req.Name,
            Address = req.Address,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            ActivationRadiusMeters = req.ActivationRadiusMeters,
            Description = req.Description,
            ImageUrlsRaw = req.ImageUrlsRaw ?? "",
            Status = req.Submit ? SubmissionStatus.Pending : SubmissionStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsUpdateRequest = shop.PublishedVersionId != null
        };
        return await _subs.AddAsync(sub);
    }

    public static string[] ParseImageUrls(string? raw) =>
        (raw ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(IsSafeImageUrl).Distinct().Take(10).ToArray();

    private static bool IsSafeImageUrl(string url) =>
        url.Length <= 2048 && !url.Any(char.IsControl) &&
        Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
        string.IsNullOrEmpty(uri.UserInfo) && !string.IsNullOrEmpty(uri.Host);

    public static void ValidateSubmission(ShopSubmissionRequest req)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(req, new ValidationContext(req), errors, true))
            throw new BusinessException(errors[0].ErrorMessage ?? "Dữ liệu địa điểm không hợp lệ.");
        if (!double.IsFinite(req.Latitude) || !double.IsFinite(req.Longitude) ||
            !double.IsFinite(req.ActivationRadiusMeters))
            throw new BusinessException("Tọa độ hoặc bán kính không hợp lệ.");
        if (req.Submit && string.IsNullOrWhiteSpace(req.Description))
            throw new BusinessException("Vui lòng nhập nội dung thuyết minh trước khi gửi duyệt.");
        var urls = (req.ImageUrlsRaw ?? "").Split(new[] { '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (urls.Length > 10 || urls.Any(x => !IsSafeImageUrl(x)))
            throw new BusinessException("Tối đa 10 đường dẫn hình ảnh HTTP/HTTPS hợp lệ, mỗi dòng một địa chỉ.");
        req.Name = req.Name.Trim();
        req.Address = req.Address.Trim();
        req.ImageUrlsRaw = string.Join("\n", urls);
    }

    public async Task<List<ShopSubmission>> ListMySubmissionsAsync(long shopId, long ownerId)
    {
        await GetMyShopAsync(shopId, ownerId);
        return await _subs.FindByShopIdAsync(shopId);
    }

    public async Task<ShopContentResponse?> GetPublishedContentAsync(long shopId)
    {
        var approved = await _subs.FindLatestApprovedByShopIdAsync(shopId);
        if (approved is null) return null;

        return new ShopContentResponse(
            approved.ShopId, approved.Name, approved.Address,
            approved.Latitude, approved.Longitude, approved.ActivationRadiusMeters,
            approved.Description, approved.ImageUrlsRaw, approved.Id);
    }
}
