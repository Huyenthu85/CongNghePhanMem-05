using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Data.Repositories;

public interface IShopSubmissionRepository
{
    Task<ShopSubmission> AddAsync(ShopSubmission sub);
    Task<ShopSubmission?> FindByIdAsync(long id);
    Task<List<ShopSubmission>> FindByStatusAsync(SubmissionStatus status);
    Task<List<ShopSubmission>> FindByShopIdAsync(long shopId);
    Task<ShopSubmission?> FindLatestApprovedByShopIdAsync(long shopId);
    Task SaveChangesAsync();
}

public class ShopSubmissionRepository : IShopSubmissionRepository
{
    private readonly AppDbContext _db;
    public ShopSubmissionRepository(AppDbContext db) => _db = db;

    public async Task<ShopSubmission> AddAsync(ShopSubmission sub)
    {
        _db.ShopSubmissions.Add(sub);
        await _db.SaveChangesAsync();
        return sub;
    }

    public Task<ShopSubmission?> FindByIdAsync(long id) =>
        _db.ShopSubmissions.Include(x => x.Shop).FirstOrDefaultAsync(x => x.Id == id);

    public Task<List<ShopSubmission>> FindByStatusAsync(SubmissionStatus status) =>
        _db.ShopSubmissions.Include(x => x.Shop)
            .Where(x => x.Status == status)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

    public Task<List<ShopSubmission>> FindByShopIdAsync(long shopId) =>
        _db.ShopSubmissions.Where(x => x.ShopId == shopId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public Task<ShopSubmission?> FindLatestApprovedByShopIdAsync(long shopId) =>
        _db.ShopSubmissions
            .Where(x => x.ShopId == shopId && x.Status == SubmissionStatus.Approved)
            .OrderByDescending(x => x.ReviewedAt)
            .FirstOrDefaultAsync();

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}