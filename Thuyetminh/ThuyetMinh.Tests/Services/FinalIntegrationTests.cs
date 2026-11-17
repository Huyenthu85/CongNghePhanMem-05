using System.Security.Claims;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Api.Services;
using ThuyetMinh.Api.Workers;
using ThuyetMinh.Business.Dtos;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;
using Xunit;

namespace ThuyetMinh.Tests.Services;

public class FinalIntegrationTests
{
    private static AppDbContext CreateDb(SqliteConnection connection) =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);

    private static User Owner() => new() { Username = "owner-test", PasswordHash = "test", Role = Role.Owner };

    [Fact]
    public async Task PublishedContent_UsesPublishedPointer_NotLatestApproval()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDb(connection);
        await db.Database.EnsureCreatedAsync();
        var owner = Owner();
        db.Users.Add(owner);
        await db.SaveChangesAsync();
        var shop = new Shop { OwnerId = owner.Id, Name = "Shop", Address = "Address",
            PublishStatus = PublishStatus.Approved };
        db.Shops.Add(shop);
        await db.SaveChangesAsync();
        var first = new ShopSubmission { ShopId = shop.Id, SubmittedById = owner.Id,
            Name = "Published", Address = "Address", Status = SubmissionStatus.Approved,
            ReviewedAt = DateTime.UtcNow.AddDays(-1) };
        var second = new ShopSubmission { ShopId = shop.Id, SubmittedById = owner.Id,
            Name = "Other", Address = "Address", Status = SubmissionStatus.Approved,
            ReviewedAt = DateTime.UtcNow };
        db.ShopSubmissions.AddRange(first, second);
        await db.SaveChangesAsync();
        shop.PublishedVersionId = first.Id;
        await db.SaveChangesAsync();
        var repo = new ShopSubmissionRepository(db);
        Assert.Equal(first.Id, (await repo.FindLatestApprovedByShopIdAsync(shop.Id))!.Id);
        shop.PublishStatus = PublishStatus.Draft;
        await db.SaveChangesAsync();
        Assert.Null(await repo.FindLatestApprovedByShopIdAsync(shop.Id));
    }

    [Fact]
    public async Task LockedThenUnlockedAccount_OldPrincipalRemainsInvalid()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDb(connection);
        await db.Database.EnsureCreatedAsync();
        var owner = Owner();
        db.Users.Add(owner);
        await db.SaveChangesAsync();
        ClaimsPrincipal Principal(int version) => new(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, owner.Id.ToString()),
            new Claim("tm_session_version", version.ToString())
        }, "test"));
        Assert.True(await AccountSessionValidator.IsValidAsync(db, Principal(0)));
        owner.Active = false;
        owner.SessionVersion++;
        await db.SaveChangesAsync();
        Assert.False(await AccountSessionValidator.IsValidAsync(db, Principal(0)));
        owner.Active = true;
        owner.SessionVersion++;
        await db.SaveChangesAsync();
        Assert.False(await AccountSessionValidator.IsValidAsync(db, Principal(0)));
        Assert.True(await AccountSessionValidator.IsValidAsync(db, Principal(2)));
    }

    [Fact]
    public async Task Cleanup_ExpiresOnlyStalePlayingSession_WithoutCharging()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDb(connection);
        await db.Database.EnsureCreatedAsync();
        var owner = Owner();
        db.Users.Add(owner);
        await db.SaveChangesAsync();
        var shop = new Shop { OwnerId = owner.Id, Name = "Shop", Address = "Address" };
        db.Shops.Add(shop);
        var visitors = Enumerable.Range(0, 3).Select(i => new AnonymousVisitor { TokenHash = $"visitor-{i}" }).ToArray();
        db.AnonymousVisitors.AddRange(visitors);
        await db.SaveChangesAsync();
        var now = DateTime.UtcNow;
        db.ListeningBalances.AddRange(visitors.Select(v => new ListeningBalance
            { AnonymousVisitorId = v.Id, RemainingSeconds = 900 }));
        db.ListeningSessions.AddRange(new ListeningSession
            { AnonymousVisitorId = visitors[0].Id, ShopId = shop.Id, LastHeartbeatAt = now.AddSeconds(-30) },
            new ListeningSession { AnonymousVisitorId = visitors[1].Id, ShopId = shop.Id, LastHeartbeatAt = now },
            new ListeningSession { AnonymousVisitorId = visitors[2].Id, ShopId = shop.Id,
                Status = ListeningSessionStatus.Paused, LastHeartbeatAt = now.AddMinutes(-2) });
        await db.SaveChangesAsync();
        Assert.Equal(1, await ListeningSessionCleanupWorker.ExpireAsync(db, now, default));
        db.ChangeTracker.Clear();
        var sessions = await db.ListeningSessions.OrderBy(s => s.Id).ToListAsync();
        Assert.Equal(ListeningSessionStatus.Ended, sessions[0].Status);
        Assert.Equal(ListeningSessionStatus.Playing, sessions[1].Status);
        Assert.Equal(ListeningSessionStatus.Paused, sessions[2].Status);
        Assert.All(await db.ListeningBalances.ToListAsync(), b => Assert.Equal(900L, b.RemainingSeconds));
        Assert.Empty(await db.ListeningTransactions.ToListAsync());
    }

    [Fact]
    public void OwnerSubmission_RejectsInvalidInputAndUnsafeImageUrls()
    {
        var request = new ShopSubmissionRequest { Name = "Valid", Address = "Valid",
            Latitude = 10, Longitude = 106, ActivationRadiusMeters = 100, Submit = true };
        Assert.Throws<BusinessException>(() => ShopService.ValidateSubmission(request));
        request.Description = "Narration";
        request.ImageUrlsRaw = "javascript:alert(1)";
        Assert.Throws<BusinessException>(() => ShopService.ValidateSubmission(request));
        request.ImageUrlsRaw = "https://example.com/image.jpg";
        ShopService.ValidateSubmission(request);
        Assert.Single(ShopService.ParseImageUrls(request.ImageUrlsRaw));
    }

    [Fact]
    public async Task OnlineStatistics_IgnorePageVisitsAndCountFreshAudioSessions()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDb(connection);
        await db.Database.EnsureCreatedAsync();
        var owner = Owner();
        var visitor = new AnonymousVisitor { TokenHash = "valid-visitor" };
        db.AddRange(owner, visitor);
        await db.SaveChangesAsync();
        var shop = new Shop { OwnerId = owner.Id, Name = "Shop", Address = "Address" };
        var language = await db.Languages.SingleAsync(l => l.Code == "vi");
        db.Shops.Add(shop);
        await db.SaveChangesAsync();
        var sub = new ShopSubmission { ShopId = shop.Id, SubmittedById = owner.Id,
            Name = shop.Name, Address = shop.Address, Status = SubmissionStatus.Approved };
        db.ShopSubmissions.Add(sub);
        await db.SaveChangesAsync();
        var translation = new Translation { ShopSubmissionId = sub.Id, LanguageId = language.Id };
        db.Translations.Add(translation);
        await db.SaveChangesAsync();
        var audio = new Audio { TranslationId = translation.Id };
        db.Audios.Add(audio);
        db.ListeningBalances.Add(new ListeningBalance { AnonymousVisitorId = visitor.Id, RemainingSeconds = 100 });
        await db.SaveChangesAsync();
        var tracker = new OnlineUserService();
        tracker.Track($"visitor-{visitor.Id}", "Guest", "Guest", "", "", true);
        Assert.False(Assert.Single(tracker.GetActualOnlineUsers(db)).IsListening);
        var session = new ListeningSession { AnonymousVisitorId = visitor.Id, ShopId = shop.Id, AudioId = audio.Id };
        db.ListeningSessions.Add(session);
        await db.SaveChangesAsync();
        Assert.True(Assert.Single(tracker.GetActualOnlineUsers(db)).IsListening);
        session.LastHeartbeatAt = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();
        Assert.False(Assert.Single(tracker.GetActualOnlineUsers(db)).IsListening);
    }
}
