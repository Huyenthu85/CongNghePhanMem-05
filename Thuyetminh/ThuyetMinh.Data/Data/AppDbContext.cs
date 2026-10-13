using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<ShopSubmission> ShopSubmissions => Set<ShopSubmission>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Username).IsUnique();

        b.Entity<Shop>()
            .HasOne(s => s.Owner)
            .WithMany(u => u.Shops)
            .HasForeignKey(s => s.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<ShopSubmission>()
            .HasOne(x => x.Shop)
            .WithMany(s => s.Submissions)
            .HasForeignKey(x => x.ShopId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ShopSubmission>()
            .HasOne(x => x.SubmittedBy)
            .WithMany()
            .HasForeignKey(x => x.SubmittedById)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<ShopSubmission>()
            .HasOne(x => x.ReviewedBy)
            .WithMany()
            .HasForeignKey(x => x.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}