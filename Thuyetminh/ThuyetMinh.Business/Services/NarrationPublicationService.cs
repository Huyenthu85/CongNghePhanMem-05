using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Business.Services;

public static class NarrationPublicationService
{
    private static readonly string[] Codes =
        { "vi", "en", "zh", "km" };

    private static IQueryable<ShopSubmission> PendingPublications(
        AppDbContext db) =>
        db.ShopSubmissions.Where(s =>
            s.Status == SubmissionStatus.Approved &&
            (s.Shop.PublishStatus == PublishStatus.Pending ||
             s.Shop.PublishStatus == PublishStatus.Approved) &&
            (s.Shop.PublishedVersionId == null ||
             s.Shop.PublishedVersionId < s.Id) &&
            !db.ShopSubmissions.Any(n =>
                n.ShopId == s.ShopId &&
                n.Id > s.Id &&
                n.Status == SubmissionStatus.Approved));

    public static IQueryable<ShopSubmission> ProcessingCandidates(
        AppDbContext db) =>
        db.ShopSubmissions.Where(s =>
            s.Status == SubmissionStatus.Approved &&
            (
                (s.Shop.PublishStatus == PublishStatus.Approved &&
                 s.Shop.PublishedVersionId == s.Id)
                ||
                (
                    (s.Shop.PublishStatus == PublishStatus.Pending ||
                     s.Shop.PublishStatus == PublishStatus.Approved) &&
                    (s.Shop.PublishedVersionId == null ||
                     s.Shop.PublishedVersionId < s.Id) &&
                    !db.ShopSubmissions.Any(n =>
                        n.ShopId == s.ShopId &&
                        n.Id > s.Id &&
                        n.Status == SubmissionStatus.Approved)
                )
            ));

    public static async Task<bool> TryPublishAsync(
        AppDbContext db,
        long submissionId,
        IReadOnlyDictionary<string, string> result,
        CancellationToken ct = default)
    {
        if (!Codes.All(code =>
            result.TryGetValue(code, out var value) &&
            value is "AudioCreated" or "AudioAlreadyExists"))
            return false;

        await using var tx =
            await db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, ct);

        var sub = await PendingPublications(db)
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == submissionId, ct);

        if (sub == null)
            return false;

        await db.Shops
            .Where(s => s.Id == sub.ShopId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(s => s.Name, sub.Name)
                .SetProperty(s => s.Address, sub.Address)
                .SetProperty(s => s.Latitude, sub.Latitude)
                .SetProperty(s => s.Longitude, sub.Longitude)
                .SetProperty(s => s.ActivationRadiusMeters,
                    sub.ActivationRadiusMeters)
                .SetProperty(s => s.PublishStatus,
                    PublishStatus.Approved)
                .SetProperty(s => s.PublishedVersionId,
                    (long?)sub.Id), ct);

        await tx.CommitAsync(ct);

        return true;
    }
}