using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Api.Workers;

public sealed class ListeningSessionCleanupWorker(IServiceScopeFactory scopes,
    ILogger<ListeningSessionCleanupWorker> logger) : BackgroundService
{
    public static Task<int> ExpireAsync(AppDbContext db, DateTime now, CancellationToken token)
    {
        var cutoff = now.AddSeconds(-ListeningService.MaxHeartbeatAgeSeconds);
        return db.ListeningSessions
            .Where(s => s.Status == ListeningSessionStatus.Playing && s.LastHeartbeatAt < cutoff)
            .ExecuteUpdateAsync(set => set
                .SetProperty(s => s.Status, ListeningSessionStatus.Ended)
                .SetProperty(s => s.EndedAt, (DateTime?)now), token);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                await ExpireAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>(),
                    DateTime.UtcNow, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception e) { logger.LogError(e, "Listening session cleanup failed."); }
        }
    }
}
