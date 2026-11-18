using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using ThuyetMinh.Api.Pages;
using ThuyetMinh.Api.Pages.Owner;
using ThuyetMinh.Business.Dtos;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;
using Xunit;

namespace ThuyetMinh.Tests.Services;

public class PrePushReviewTests
{
    [Fact]
    public void Registration_RejectsInvalidInput()
    {
        var input = new RegisterInput
        {
            Username = "owner99",
            Password = "secret123",
            ConfirmPassword = "secret123",
            PhoneNumber = "0912345678",
            AddressLine = "123 Main",
            Ward = "Ward",
            City = "City"
        };

        Assert.Null(input.GetValidationError());

        input.PhoneNumber = "123";
        Assert.NotNull(input.GetValidationError());

        input.PhoneNumber = "0912345678";
        input.Password = input.ConfirmPassword = "123";
        Assert.NotNull(input.GetValidationError());

        input.Password = input.ConfirmPassword = "secret123";
        input.City = "";
        Assert.NotNull(input.GetValidationError());
    }

    [Fact]
    public void Profile_ShowsLatestSubmissionStatus()
    {
        var page = new ProfileModel(
            Mock.Of<IUserRepository>(),
            Mock.Of<IShopService>());

        var shop = new Shop
        {
            Id = 1,
            PublishStatus = PublishStatus.Approved,
            PublishedVersionId = 10
        };

        page.LatestSubmissions[1] = new ShopSubmission
        {
            Id = 11,
            Status = SubmissionStatus.Rejected,
            RejectionReason = "Invalid content"
        };

        Assert.Equal(
            SubmissionStatus.Rejected,
            page.GetReviewStatus(shop));

        page.LatestSubmissions[1].Status =
            SubmissionStatus.Pending;

        Assert.Equal(
            SubmissionStatus.Pending,
            page.GetReviewStatus(shop));
    }

    private static async Task<(User Owner, User Admin, Shop Shop)> Seed(
        AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        var owner = new User
        {
            Username = "owner-test",
            PasswordHash = "test",
            Role = Role.Owner
        };

        var admin = new User
        {
            Username = "admin-test",
            PasswordHash = "test",
            Role = Role.Admin
        };

        db.AddRange(owner, admin);
        await db.SaveChangesAsync();

        var shop = new Shop
        {
            OwnerId = owner.Id,
            Name = "Original",
            Address = "Address",
            Latitude = 10,
            Longitude = 106,
            ActivationRadiusMeters = 100
        };

        db.Shops.Add(shop);
        await db.SaveChangesAsync();

        return (owner, admin, shop);
    }

    private static ShopSubmission CreateSubmission(
        Shop shop, User owner, string name)
        => new()
        {
            ShopId = shop.Id,
            SubmittedById = owner.Id,
            Name = name,
            Address = "Address",
            Latitude = 10,
            Longitude = 106,
            ActivationRadiusMeters = 100,
            Description = "Narration content",
            Status = SubmissionStatus.Pending
        };

    private static ReviewService Reviewer(AppDbContext db)
        => new(
            new ShopSubmissionRepository(db),
            new ShopRepository(db),
            new UserRepository(db),
            db);

    private static Dictionary<string, string> AudioResults(
        string khmerResult = "AudioCreated")
        => new()
        {
            ["vi"] = "AudioCreated",
            ["en"] = "AudioCreated",
            ["zh"] = "AudioCreated",
            ["km"] = khmerResult
        };

    [Fact]
    public async Task NewShop_PublishesOnlyWhenAllAudioReady()
    {
        await using var cn =
            new SqliteConnection("Data Source=:memory:");

        await cn.OpenAsync();

        await using var db = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(cn).Options);

        var (owner, admin, shop) = await Seed(db);

        var sub = CreateSubmission(shop, owner, "New Shop");

        db.ShopSubmissions.Add(sub);
        await db.SaveChangesAsync();

        await Reviewer(db).ReviewAsync(
            sub.Id, admin.Id,
            new ReviewRequest { Approve = true });

        Assert.Equal(PublishStatus.Pending, shop.PublishStatus);
        Assert.Null(shop.PublishedVersionId);

        Assert.False(
            await NarrationPublicationService.TryPublishAsync(
                db, sub.Id, AudioResults("Failed")));

        Assert.True(
            await NarrationPublicationService.TryPublishAsync(
                db, sub.Id, AudioResults()));

        await db.Entry(shop).ReloadAsync();

        Assert.Equal(PublishStatus.Approved, shop.PublishStatus);
        Assert.Equal(sub.Id, shop.PublishedVersionId);
    }

    [Fact]
    public async Task UpdatedShop_KeepsOldVersionUntilAudioReady()
    {
        await using var cn =
            new SqliteConnection("Data Source=:memory:");

        await cn.OpenAsync();

        await using var db = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(cn).Options);

        var (owner, admin, shop) = await Seed(db);

        var old = CreateSubmission(shop, owner, "Original");
        old.Status = SubmissionStatus.Approved;

        db.ShopSubmissions.Add(old);
        await db.SaveChangesAsync();

        shop.PublishStatus = PublishStatus.Approved;
        shop.PublishedVersionId = old.Id;

        var next = CreateSubmission(shop, owner, "Updated");

        db.ShopSubmissions.Add(next);
        await db.SaveChangesAsync();

        await Reviewer(db).ReviewAsync(
            next.Id, admin.Id,
            new ReviewRequest { Approve = true });

        Assert.Equal(old.Id, shop.PublishedVersionId);
        Assert.Equal("Original", shop.Name);

        Assert.True(
            await NarrationPublicationService
                .ProcessingCandidates(db)
                .AnyAsync(s => s.Id == next.Id));

        Assert.False(
            await NarrationPublicationService.TryPublishAsync(
                db, next.Id, AudioResults("Failed")));

        await db.Entry(shop).ReloadAsync();

        Assert.Equal(old.Id, shop.PublishedVersionId);

        Assert.True(
            await NarrationPublicationService.TryPublishAsync(
                db, next.Id, AudioResults()));

        await db.Entry(shop).ReloadAsync();

        Assert.Equal(next.Id, shop.PublishedVersionId);
        Assert.Equal("Updated", shop.Name);

        Assert.False(
            await NarrationPublicationService.TryPublishAsync(
                db, old.Id, AudioResults()));
    }

    [Fact]
    public async Task OlderSubmission_CannotReplaceNewerApproval()
    {
        await using var cn =
            new SqliteConnection("Data Source=:memory:");

        await cn.OpenAsync();

        await using var db = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(cn).Options);

        var (owner, admin, shop) = await Seed(db);

        var old = CreateSubmission(shop, owner, "Old");
        db.ShopSubmissions.Add(old);
        await db.SaveChangesAsync();

        var next = CreateSubmission(shop, owner, "New");
        db.ShopSubmissions.Add(next);
        await db.SaveChangesAsync();

        var service = Reviewer(db);

        await service.ReviewAsync(
            next.Id, admin.Id,
            new ReviewRequest { Approve = true });

        await Assert.ThrowsAsync<BusinessException>(
            () => service.ReviewAsync(
                old.Id, admin.Id,
                new ReviewRequest { Approve = true }));

        await service.ReviewAsync(
            old.Id, admin.Id,
            new ReviewRequest
            {
                Approve = false,
                RejectionReason = "Old version"
            });

        Assert.Equal(SubmissionStatus.Rejected, old.Status);
        Assert.Equal(SubmissionStatus.Approved, next.Status);
        Assert.Equal(PublishStatus.Pending, shop.PublishStatus);
    }
}