using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Moq;
using ThuyetMinh.Api.Controllers;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
using Xunit;

namespace ThuyetMinh.Tests.Services;

public class ListeningServiceTests
{
    private static async Task<(SqliteConnection Connection, AppDbContext Db, long VisitorId, long ShopId)> SetupAsync(long seconds)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var owner = new User { Username = "demo-owner", PasswordHash = "test", Role = Role.Owner };
        db.Users.Add(owner);
        await db.SaveChangesAsync();
        var shop = new Shop { OwnerId = owner.Id, Name = "Địa điểm mẫu", Address = "Demo", PublishStatus = PublishStatus.Approved };
        var visitor = new AnonymousVisitor { TokenHash = Guid.NewGuid().ToString("N") };
        db.Shops.Add(shop);
        db.AnonymousVisitors.Add(visitor);
        await db.SaveChangesAsync();
        db.ListeningBalances.Add(new ListeningBalance { AnonymousVisitorId = visitor.Id, RemainingSeconds = seconds });
        await db.SaveChangesAsync();
        return (connection, db, visitor.Id, shop.Id);
    }

    [Fact]
    public async Task StartAsync_NoBalance_DoesNotStart()
    {
        var test = await SetupAsync(0);
        await using var connection = test.Connection;
        await using var db = test.Db;
        var service = new ListeningService(db);
        Assert.Null(await service.StartAsync(test.VisitorId, test.ShopId));
        Assert.Equal(0, await db.ListeningSessions.CountAsync());
    }

    [Fact]
    public async Task Heartbeat_DebitsOnlyElapsedServerTime_AndPauseStopsCharging()
    {
        var test = await SetupAsync(900);
        await using var connection = test.Connection;
        await using var db = test.Db;
        var service = new ListeningService(db);
        var started = await service.StartAsync(test.VisitorId, test.ShopId);
        Assert.NotNull(started);
        var session = await db.ListeningSessions.SingleAsync();
        session.LastHeartbeatAt = DateTime.UtcNow.AddSeconds(-4);
        await db.SaveChangesAsync();
        var heartbeat = await service.UpdateAsync(test.VisitorId, session.Id, "heartbeat");
        Assert.NotNull(heartbeat);
        Assert.InRange(heartbeat.RemainingSeconds, 892L, 899L);
        var paused = await service.UpdateAsync(test.VisitorId, session.Id, "pause");
        Assert.NotNull(paused);
        var afterPause = paused.RemainingSeconds;
        session.LastHeartbeatAt = DateTime.UtcNow.AddSeconds(-20);
        await db.SaveChangesAsync();
        var whilePaused = await service.UpdateAsync(test.VisitorId, session.Id, "heartbeat");
        Assert.NotNull(whilePaused);
        Assert.Equal(afterPause, whilePaused.RemainingSeconds);
        var ledger = await db.ListeningTransactions.ToListAsync();
        Assert.Contains(ledger, x => x.Type == ListeningTransactionType.ListeningDebit && x.ChangeSeconds < 0);
    }

    [Fact]
    public async Task StartAfterPause_DoesNotChargePausedTime()
    {
        var test = await SetupAsync(900);
        await using var connection = test.Connection;
        await using var db = test.Db;
        var service = new ListeningService(db);
        var first = (await service.StartAsync(test.VisitorId, test.ShopId))!;
        var paused = (await service.UpdateAsync(test.VisitorId, first.SessionId, "pause"))!;
        var session = await db.ListeningSessions.SingleAsync();
        session.LastHeartbeatAt = DateTime.UtcNow.AddMinutes(-5);
        await db.SaveChangesAsync();
        var next = (await service.StartAsync(test.VisitorId, test.ShopId))!;
        Assert.Equal(paused.RemainingSeconds, next.RemainingSeconds);
        Assert.Equal(ListeningSessionStatus.Ended, session.Status);
    }

    [Fact]
    public async Task ExpiredHeartbeat_EndsPlayingSession()
    {
        var test = await SetupAsync(900);
        await using var connection = test.Connection;
        await using var db = test.Db;
        var service = new ListeningService(db);
        var first = (await service.StartAsync(test.VisitorId, test.ShopId))!;
        var session = await db.ListeningSessions.SingleAsync();
        session.LastHeartbeatAt = DateTime.UtcNow.AddMinutes(-2);
        await db.SaveChangesAsync();
        var result = (await service.UpdateAsync(test.VisitorId, first.SessionId, "heartbeat"))!;
        Assert.Equal("Ended", result.Status);
        Assert.InRange(result.RemainingSeconds, 892L, 900L);
    }

    private static async Task<Audio> CreatePublishedAudioAsync(AppDbContext db)
    {
        var shop = await db.Shops.SingleAsync();
        var sub = new ShopSubmission { ShopId = shop.Id, SubmittedById = shop.OwnerId,
            Name = shop.Name, Address = shop.Address, Status = SubmissionStatus.Approved };
        var language = await db.Languages.SingleAsync(x => x.Code == "vi");
        db.ShopSubmissions.Add(sub);
        await db.SaveChangesAsync();
        shop.PublishedVersionId = sub.Id;
        var translation = new Translation { ShopSubmissionId = sub.Id, LanguageId = language.Id,
            TranslatedText = "test", Status = "Success" };
        db.Translations.Add(translation);
        await db.SaveChangesAsync();
        var audio = new Audio { TranslationId = translation.Id, FilePath = "/audio/test.mp3",
            Status = "Success", DurationSeconds = 120 };
        db.Audios.Add(audio);
        await db.SaveChangesAsync();
        return audio;
    }

    [Fact]
    public async Task ReportedPlayback_ChargesExactlyFortySeconds_AndRetriesDoNotDoubleCharge()
    {
        var test = await SetupAsync(900);
        await using var connection = test.Connection;
        await using var db = test.Db;
        var audio = await CreatePublishedAudioAsync(db);
        var service = new ListeningService(db);
        var start = (await service.StartAsync(test.VisitorId, test.ShopId, audio.Id))!;
        var session = await db.ListeningSessions.SingleAsync();
        session.StartedAt = DateTime.UtcNow.AddSeconds(-50);
        session.LastHeartbeatAt = DateTime.UtcNow.AddSeconds(-10);
        await db.SaveChangesAsync();
        var loading = (await service.UpdateAsync(test.VisitorId, session.Id, "heartbeat", 0))!;
        Assert.Equal(900L, loading.RemainingSeconds);
        for (long total = 10; total <= 40; total += 10)
        {
            session.LastHeartbeatAt = DateTime.UtcNow.AddSeconds(-10);
            await db.SaveChangesAsync();
            var heartbeat = (await service.UpdateAsync(test.VisitorId, session.Id, "heartbeat", total))!;
            Assert.Equal(900 - total, heartbeat.RemainingSeconds);
            Assert.Equal(total, heartbeat.ChargedSeconds);
        }
        var duplicate = (await service.UpdateAsync(test.VisitorId, session.Id, "heartbeat", 40))!;
        Assert.Equal(860L, duplicate.RemainingSeconds);
        await service.UpdateAsync(test.VisitorId, session.Id, "pause", 40);
        session.LastHeartbeatAt = DateTime.UtcNow.AddSeconds(-15);
        await db.SaveChangesAsync();
        var paused = (await service.UpdateAsync(test.VisitorId, session.Id, "resume", 40))!;
        Assert.Equal(860L, paused.RemainingSeconds);
        session.LastHeartbeatAt = DateTime.UtcNow.AddSeconds(-4);
        await db.SaveChangesAsync();
        var ended = (await service.UpdateAsync(test.VisitorId, session.Id, "end", 44))!;
        Assert.Equal(856L, ended.RemainingSeconds);
        Assert.Equal(44L, ended.ChargedSeconds);
        Assert.Equal(-44L, await db.ListeningTransactions.SumAsync(x => x.ChangeSeconds));
    }

    [Fact]
    public async Task ReportedPlayback_RejectsNegativeAndBoundsImpossibleDuration()
    {
        var test = await SetupAsync(900);
        await using var connection = test.Connection;
        await using var db = test.Db;
        var audio = await CreatePublishedAudioAsync(db);
        var service = new ListeningService(db);
        var start = (await service.StartAsync(test.VisitorId, test.ShopId, audio.Id))!;
        Assert.Null(await service.UpdateAsync(test.VisitorId, start.SessionId, "heartbeat", -1));
        var session = await db.ListeningSessions.SingleAsync();
        session.StartedAt = DateTime.UtcNow.AddSeconds(-3);
        session.LastHeartbeatAt = session.StartedAt;
        await db.SaveChangesAsync();
        var bounded = (await service.UpdateAsync(test.VisitorId, start.SessionId, "heartbeat", 900))!;
        Assert.InRange(bounded.ChargedSeconds, 3L, 6L);
        session.LastHeartbeatAt = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();
        var expired = (await service.UpdateAsync(test.VisitorId, start.SessionId, "heartbeat", 900))!;
        Assert.Equal("Ended", expired.Status);
        Assert.Equal(bounded.RemainingSeconds, expired.RemainingSeconds);
    }

    [Fact]
    public async Task StreamAudio_RequiresMatchingActivePublishedSession()
    {
        var test = await SetupAsync(900);
        await using var connection = test.Connection;
        await using var db = test.Db;
        var shop = await db.Shops.SingleAsync();
        var visitor = await db.AnonymousVisitors.SingleAsync();
        var audio = await CreatePublishedAudioAsync(db);
        var root = Path.Combine(Path.GetTempPath(), "tm-audio-test-" + Guid.NewGuid().ToString("N"));
        var dir = Path.Combine(root, "audio");
        Directory.CreateDirectory(dir);
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(dir, "test.mp3"), new byte[] { 1, 2, 3 });
            var visitors = new Mock<IAnonymousVisitorService>();
            visitors.Setup(x => x.FindExistingAsync(It.IsAny<string?>())).ReturnsAsync(visitor);
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(x => x.WebRootPath).Returns(root);
            var service = new ListeningService(db);
            var controller = new ListeningController(service, visitors.Object, db, environment.Object,
                new ConfigurationBuilder().Build())
            { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
            Assert.IsType<NotFoundResult>(await controller.StreamAudio(99999));
            var started = (await service.StartAsync(visitor.Id, shop.Id, audio.Id))!;
            Assert.Equal(audio.Id, (await db.ListeningSessions.SingleAsync()).AudioId);
            Assert.IsType<PhysicalFileResult>(await controller.StreamAudio(started.SessionId));
            visitors.Setup(x => x.FindExistingAsync(It.IsAny<string?>()))
                .ReturnsAsync(new AnonymousVisitor { Id = visitor.Id + 999 });
            Assert.IsType<NotFoundResult>(await controller.StreamAudio(started.SessionId));
            visitors.Setup(x => x.FindExistingAsync(It.IsAny<string?>())).ReturnsAsync(visitor);
            await service.UpdateAsync(visitor.Id, started.SessionId, "pause");
            Assert.Equal(403, Assert.IsType<StatusCodeResult>(
                await controller.StreamAudio(started.SessionId)).StatusCode);
            await service.UpdateAsync(visitor.Id, started.SessionId, "resume");
            shop.PublishStatus = PublishStatus.Draft;
            await db.SaveChangesAsync();
            Assert.IsType<NotFoundResult>(await controller.StreamAudio(started.SessionId));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task ReplayEightSeconds_DoesNotDoubleDebit()
    {
        var test = await SetupAsync(900);
        await using var connection = test.Connection;
        await using var db = test.Db;

        var audio = await CreatePublishedAudioAsync(db);
        var service = new ListeningService(db);

        for (int i = 1; i <= 2; i++)
        {
            var started = (await service.StartAsync(
                test.VisitorId, test.ShopId, audio.Id))!;

            var session = await db.ListeningSessions
                .SingleAsync(x => x.Id == started.SessionId);

            session.StartedAt = DateTime.UtcNow.AddSeconds(-18);
            session.LastHeartbeatAt = DateTime.UtcNow.AddSeconds(-10);
            await db.SaveChangesAsync();

            var ended = (await service.UpdateAsync(
                test.VisitorId, session.Id, "end", 8))!;

            Assert.Equal("Ended", ended.Status);
            Assert.Equal(8L, ended.ChargedSeconds);
            Assert.Equal(900L - i * 8, ended.RemainingSeconds);

            var duplicate = (await service.UpdateAsync(
                test.VisitorId, session.Id, "end", 16))!;

            Assert.Equal(ended.RemainingSeconds, duplicate.RemainingSeconds);
        }

        Assert.Equal(-16L, await db.ListeningTransactions
            .SumAsync(x => x.ChangeSeconds));

        Assert.Equal(0, await db.ListeningSessions.CountAsync(
            x => x.Status != ListeningSessionStatus.Ended));
    }

    [Fact]
    public async Task NewSession_ClosesOldSessionsWithoutExtraDebit()
    {
        var test = await SetupAsync(900);
        await using var connection = test.Connection;
        await using var db = test.Db;

        var audio = await CreatePublishedAudioAsync(db);
        var service = new ListeningService(db);

        var old = (await service.StartAsync(
            test.VisitorId, test.ShopId))!;

        var oldSession = await db.ListeningSessions
            .SingleAsync(x => x.Id == old.SessionId);

        oldSession.LastHeartbeatAt = DateTime.UtcNow.AddSeconds(-16);

        await db.SaveChangesAsync();

        var next = (await service.StartAsync(
            test.VisitorId, test.ShopId, audio.Id))!;

        Assert.Equal(900L, next.RemainingSeconds);

        Assert.Equal(1, await db.ListeningSessions.CountAsync(
            x => x.Status != ListeningSessionStatus.Ended));

        Assert.Equal(ListeningSessionStatus.Ended, oldSession.Status);

        var late = (await service.UpdateAsync(
            test.VisitorId, old.SessionId, "end", 16))!;

        Assert.Equal(900L, late.RemainingSeconds);
        Assert.Empty(await db.ListeningTransactions.ToListAsync());
    }

    [Fact]
    public async Task ExpiredLegacySession_DoesNotDebit()
    {
        var test = await SetupAsync(900);
        await using var connection = test.Connection;
        await using var db = test.Db;

        var service = new ListeningService(db);

        var started = (await service.StartAsync(
            test.VisitorId, test.ShopId))!;

        var session = await db.ListeningSessions.SingleAsync();
        session.LastHeartbeatAt = DateTime.UtcNow.AddMinutes(-2);
        await db.SaveChangesAsync();

        var result = (await service.UpdateAsync(
            test.VisitorId, started.SessionId, "heartbeat"))!;

        Assert.Equal("Ended", result.Status);
        Assert.Equal(900L, result.RemainingSeconds);
        Assert.Equal(0L, result.ChargedSeconds);
    }
}
