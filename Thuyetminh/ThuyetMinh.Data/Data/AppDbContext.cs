using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<ShopSubmission> ShopSubmissions => Set<ShopSubmission>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Translation> Translations => Set<Translation>();
    public DbSet<Audio> Audios => Set<Audio>();
    public DbSet<ProcessingJob> ProcessingJobs => Set<ProcessingJob>();
    public DbSet<AnonymousVisitor> AnonymousVisitors => Set<AnonymousVisitor>();
    public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();

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
        b.Entity<Language>()
            .HasIndex(x => x.Code)
            .IsUnique();

        b.Entity<Translation>()
            .HasOne(x => x.ShopSubmission)
            .WithMany()
            .HasForeignKey(x => x.ShopSubmissionId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Translation>()
            .HasOne(x => x.Language)
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Translation>()
            .HasIndex(x => new { x.ShopSubmissionId, x.LanguageId })
            .IsUnique();

        b.Entity<Audio>()
            .HasOne<Translation>()
            .WithMany()
            .HasForeignKey(x => x.TranslationId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<ProcessingJob>()
            .HasOne(x => x.ShopSubmission)
            .WithMany()
            .HasForeignKey(x => x.ShopSubmissionId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<ProcessingJob>()
            .HasOne(x => x.Language)
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
            
        b.Entity<AnonymousVisitor>()
            .HasIndex(x => x.TokenHash)
            .IsUnique();

        b.Entity<PaymentOrder>()
            .HasOne(x => x.AnonymousVisitor)
            .WithMany()
            .HasForeignKey(x => x.AnonymousVisitorId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<PaymentOrder>()
            .HasIndex(x => x.OrderCode)
            .IsUnique();

        b.Entity<PaymentOrder>()
            .HasIndex(x => x.ProviderTransactionId)
            .IsUnique();

        b.Entity<PaymentOrder>()
            .Property(x => x.Status)
            .HasConversion<string>();


        b.Entity<Language>().HasData(
            new Language { Id = 1, Code = "vi", Name = "Tiếng Việt", IsActive = true },
            new Language { Id = 2, Code = "en", Name = "Tiếng Anh", IsActive = true },
            new Language { Id = 3, Code = "zh", Name = "Tiếng Trung", IsActive = true },
            new Language { Id = 4, Code = "km", Name = "Tiếng Khmer", IsActive = true }
        );


    }
}