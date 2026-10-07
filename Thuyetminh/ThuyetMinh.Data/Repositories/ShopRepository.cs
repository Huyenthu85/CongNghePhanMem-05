using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Data.Repositories;

public interface IShopRepository
{
    Task<List<Shop>> FindByOwnerIdAsync(long ownerId);
    Task<Shop?> FindByIdAndOwnerIdAsync(long id, long ownerId);
    Task<Shop?> FindByIdAsync(long id);
    Task SaveChangesAsync();
}

public class ShopRepository : IShopRepository
{
    private readonly AppDbContext _db;
    public ShopRepository(AppDbContext db) => _db = db;

    public Task<List<Shop>> FindByOwnerIdAsync(long ownerId) =>
        _db.Shops.Where(s => s.OwnerId == ownerId).ToListAsync();

    public Task<Shop?> FindByIdAndOwnerIdAsync(long id, long ownerId) =>
        _db.Shops.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == ownerId);

    public Task<Shop?> FindByIdAsync(long id) =>
        _db.Shops.FirstOrDefaultAsync(s => s.Id == id);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}