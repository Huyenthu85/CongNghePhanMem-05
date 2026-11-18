using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Api.Workers;

public class NarrationProcessingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NarrationProcessingWorker> _logger;

    public NarrationProcessingWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<NarrationProcessingWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessNextAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Narration background processing failed.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(30),
                stoppingToken);
        }
    }

    private async Task ProcessNextAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var submissionIds = await NarrationPublicationService.ProcessingCandidates(db)
            .AsNoTracking()
            .Where(s =>
                s.Description != null &&
                s.Description.Trim() != "")
            .OrderBy(s => s.Id)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        foreach (var submissionId in submissionIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, string> result;
            try
            {
                using var jobScope = _scopeFactory.CreateScope();
                result = await jobScope.ServiceProvider.GetRequiredService<NarrationProcessingService>()
                    .ProcessAsync(submissionId, cancellationToken, automatic: true);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Skipping failed submission {SubmissionId} this cycle.", submissionId);
                continue;
            }

            foreach (var item in result)
            {
                if (item.Value == "AudioAlreadyExists" ||
                    item.Value == "WaitingForRetry" ||
                    item.Value == "RetryLimitReached" ||
                    item.Value == "AlreadyProcessing")
                    continue;

                _logger.LogInformation(
                    "Submission {SubmissionId} - {Language}: {Result}",
                    submissionId,
                    item.Key,
                    item.Value);
            }
        }
    }
}
