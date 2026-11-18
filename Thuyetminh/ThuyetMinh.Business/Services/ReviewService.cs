using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data;
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
    private readonly AppDbContext _db;

    public ReviewService(IShopSubmissionRepository subs, IShopRepository shops, IUserRepository users, AppDbContext db)
    {
        _subs = subs;
        _shops = shops;
        _users = users;
        _db = db;
    }

    public Task<List<ShopSubmission>> ListPendingAsync() =>
        _subs.FindByStatusAsync(SubmissionStatus.Pending);

    public async Task<ShopSubmission> ReviewAsync(long submissionId, long adminId, ReviewRequest req)
    {
        await using var transaction =
            await _db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

        var admin = await _users.FindByIdAsync(adminId)
            ?? throw new UnauthorizedAccessException("KhĂ´ng pháº£i admin");

        if (admin.Role != Role.Admin || !admin.Active)
            throw new UnauthorizedAccessException("KhĂ´ng cĂ³ quyá»n duyá»‡t bĂ i");

        var sub = await _subs.FindByIdAsync(submissionId)
            ?? throw new KeyNotFoundException("KhĂ´ng tĂ¬m tháº¥y bĂ i gá»­i");

        if (sub.Status != SubmissionStatus.Pending)
            throw new BusinessException("Chá»‰ duyá»‡t Ä‘Æ°á»£c bĂ i Ä‘ang chá»");

        if (req.Approve)
        {
            if (sub.Shop.PublishedVersionId is long publishedId
                && publishedId > sub.Id)
            {
                throw new BusinessException(
                    "Đã có phiên bản mới hơn được công bố. Vui lòng từ chối bài gửi cũ.");
            }

            if (await _db.ShopSubmissions.AnyAsync(x =>
                x.ShopId == sub.ShopId
                && x.Id > sub.Id
                && x.Status == SubmissionStatus.Approved))
            {
                throw new BusinessException(
                    "Đã có bài gửi mới hơn được duyệt. Vui lòng từ chối bài gửi cũ.");
            }

            ShopService.ValidateSubmission(new ShopSubmissionRequest
            {
                Name = sub.Name, Address = sub.Address, Latitude = sub.Latitude,
                Longitude = sub.Longitude, ActivationRadiusMeters = sub.ActivationRadiusMeters,
                Description = sub.Description, ImageUrlsRaw = sub.ImageUrlsRaw, Submit = true
            });
            sub.Status = SubmissionStatus.Approved;
            sub.RejectionReason = null;

            var shop = sub.Shop;
            // Keep the previous public version until the new audio is ready.
            if (shop.PublishStatus != PublishStatus.Approved
                || shop.PublishedVersionId == null)
            {
                shop.PublishStatus = PublishStatus.Pending;
            }

            if (!string.IsNullOrWhiteSpace(sub.Description))
            {
                var codes = new[] { "vi", "en", "zh", "km" };

                var languageIds = await _db.Languages
                    .Where(l => l.IsActive && codes.Contains(l.Code))
                    .Select(l => l.Id)
                    .ToListAsync();

                var queuedIds = await _db.ProcessingJobs
                    .Where(j => j.ShopSubmissionId == sub.Id &&
                        (j.Status == "Pending" ||
                         j.Status == "Processing"))
                    .Select(j => j.LanguageId)
                    .ToListAsync();

                foreach (var languageId in languageIds.Except(queuedIds))
                {
                    _db.ProcessingJobs.Add(new ProcessingJob
                    {
                        ShopSubmissionId = sub.Id,
                        LanguageId = languageId,
                        Status = "Pending"
                    });
                }
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(req.RejectionReason))
                throw new BusinessException("Pháº£i nháº­p lĂ½ do tá»« chá»‘i");

            sub.Status = SubmissionStatus.Rejected;
            sub.RejectionReason = req.RejectionReason;

            var shop = sub.Shop;
            if (shop.PublishedVersionId is null
                && !await _db.ShopSubmissions.AnyAsync(x =>
                    x.ShopId == shop.Id
                    && x.Id != sub.Id
                    && x.Status == SubmissionStatus.Approved))
                shop.PublishStatus = PublishStatus.Rejected;
        }

        sub.ReviewedById = adminId;
        sub.ReviewedAt = DateTime.UtcNow;
        sub.UpdatedAt = DateTime.UtcNow;

        await _subs.SaveChangesAsync();
        await _shops.SaveChangesAsync();
        await transaction.CommitAsync();
        return sub;
    }

    public async Task UpdatePublishStatusAsync(long shopId, PublishStatus status)
    {
        var shop = await _shops.FindByIdAsync(shopId)
            ?? throw new KeyNotFoundException("Shop khĂ´ng tá»“n táº¡i");
        if (!Enum.IsDefined(status))
            throw new BusinessException("Trạng thái xuất bản không hợp lệ.");
        if (status == PublishStatus.Approved &&
            !await _db.ShopSubmissions.AnyAsync(s => s.ShopId == shop.Id &&
                s.Id == shop.PublishedVersionId && s.Status == SubmissionStatus.Approved))
            throw new BusinessException("Quán chưa có phiên bản được duyệt.");
        shop.PublishStatus = status;
        await _shops.SaveChangesAsync();
    }
}
