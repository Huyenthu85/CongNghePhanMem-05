using ThuyetMinh.Business.Dtos;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;

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