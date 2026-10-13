using ThuyetMinh.Business.Dtos;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;

namespace ThuyetMinh.Business.Services;

public interface IReviewService
{
    Task<List<ShopSubmission>> ListPendingAsync();
    Task<ShopSubmission> ReviewAsync(long submissionId, long adminId, ReviewRequest req);
    Task UpdatePublishStatusAsync(long shopId, PublishStatus status);
}

public class ReviewService : IReviewService
{
    private readonly IShopSubmissionRepository _subs;
    private readonly IShopRepository _shops;
    private readonly IUserRepository _users;

    public ReviewService(IShopSubmissionRepository subs, IShopRepository shops, IUserRepository users)
    {
        _subs = subs;
        _shops = shops;
        _users = users;
    }

    public Task<List<ShopSubmission>> ListPendingAsync() =>
        _subs.FindByStatusAsync(SubmissionStatus.Pending);

    public async Task<ShopSubmission> ReviewAsync(long submissionId, long adminId, ReviewRequest req)
    {
        var admin = await _users.FindByIdAsync(adminId)
            ?? throw new UnauthorizedAccessException("Không phải admin");

        if (admin.Role != Role.Admin)
            throw new UnauthorizedAccessException("Không có quyền duyệt bài");

        var sub = await _subs.FindByIdAsync(submissionId)
            ?? throw new KeyNotFoundException("Không tìm thấy bài gửi");

        if (sub.Status != SubmissionStatus.Pending)
            throw new BusinessException("Chỉ duyệt được bài đang chờ");

        if (req.Approve)
        {
            sub.Status = SubmissionStatus.Approved;
            sub.RejectionReason = null;

            var shop = sub.Shop;
            shop.Name = sub.Name;
            shop.Address = sub.Address;
            shop.Latitude = sub.Latitude;
            shop.Longitude = sub.Longitude;
            shop.ActivationRadiusMeters = sub.ActivationRadiusMeters;
            shop.PublishStatus = PublishStatus.Approved;
            shop.PublishedVersionId = sub.Id;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(req.RejectionReason))
                throw new BusinessException("Phải nhập lý do từ chối");

            sub.Status = SubmissionStatus.Rejected;
            sub.RejectionReason = req.RejectionReason;

            var shop = sub.Shop;
            if (shop.PublishedVersionId is null)
                shop.PublishStatus = PublishStatus.Rejected;
        }

        sub.ReviewedById = adminId;
        sub.ReviewedAt = DateTime.UtcNow;
        sub.UpdatedAt = DateTime.UtcNow;

        await _subs.SaveChangesAsync();
        await _shops.SaveChangesAsync();
        return sub;
    }

    public async Task UpdatePublishStatusAsync(long shopId, PublishStatus status)
    {
        var shop = await _shops.FindByIdAsync(shopId)
            ?? throw new KeyNotFoundException("Shop không tồn tại");
        shop.PublishStatus = status;
        await _shops.SaveChangesAsync();
    }
}