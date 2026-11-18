using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Business.Services;

public class NarrationProcessingService
{
    private readonly AppDbContext _db;
    private readonly IMachineTranslationService _translator;
    private readonly ITtsService _tts;
    private readonly string _webRoot;

    private static readonly SemaphoreSlim Gate = new(1, 1);

    private static readonly string[] Codes =
        { "vi", "en", "zh", "km" };

    public NarrationProcessingService(
        AppDbContext db,
        TranslationService translations,
        IMachineTranslationService translator,
        AudioService audios,
        ITtsService tts, IConfiguration configuration)
    {
        _db = db;
        _translator = translator;
        _tts = tts;
        _webRoot = configuration["Narration:WebRoot"] ??
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public async Task<Dictionary<string, string>> ProcessAsync(
        long submissionId,
        CancellationToken cancellationToken = default, bool automatic = false)
    {
        await Gate.WaitAsync(cancellationToken);

        try
        {
            return await ProcessCoreAsync(
                submissionId, cancellationToken, automatic);
        }
        finally
        {
            Gate.Release();
        }
    }

    private async Task<Dictionary<string, string>> ProcessCoreAsync(
        long submissionId,
        CancellationToken cancellationToken,
        bool automatic)
    {
        var sub = await _db.ShopSubmissions
            .AsNoTracking()
            .Include(x => x.Shop)
            .FirstOrDefaultAsync(
                x => x.Id == submissionId,
                cancellationToken);

        if (sub == null)
            throw new KeyNotFoundException(
                "Không tìm thấy bài gửi.");

        if (!await NarrationPublicationService
            .ProcessingCandidates(_db)
            .AnyAsync(s => s.Id == sub.Id, cancellationToken))
        {
            throw new InvalidOperationException(
                "Bài gửi chưa được duyệt hoặc không còn là phiên bản cần xử lý.");
        }

        if (string.IsNullOrWhiteSpace(sub.Description))
            throw new InvalidOperationException(
                "Bài gửi chưa có nội dung thuyết minh.");

        var result = new Dictionary<string, string>();

        var webRoot = Path.GetFullPath(_webRoot);

        var audioDirectory = Path.Combine(
            webRoot, "audio", "generated");

        Directory.CreateDirectory(audioDirectory);

        foreach (var code in Codes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int? jobId = null;

            try
            {
                var language = await _db.Languages
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Code == code && x.IsActive,
                        cancellationToken);

                if (language == null)
                {
                    result[code] = "Failed: Ngôn ngữ không hoạt động.";
                    continue;
                }

                var job = await _db.ProcessingJobs
                    .Where(x =>
                        x.ShopSubmissionId == submissionId &&
                        x.LanguageId == language.Id)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (job == null)
                {
                    job = new ProcessingJob
                    {
                        ShopSubmissionId = submissionId,
                        LanguageId = language.Id,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };

                    _db.ProcessingJobs.Add(job);
                    await _db.SaveChangesAsync(cancellationToken);
                }

                var now = DateTime.UtcNow;

                if (job.Status == "Success")
                {
                    var existingTranslation = await _db.Translations
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.ShopSubmissionId == submissionId &&
                            x.LanguageId == language.Id,
                            cancellationToken);

                    if (existingTranslation != null &&
                        existingTranslation.Status == "Success" &&
                        !string.IsNullOrWhiteSpace(
                            existingTranslation.TranslatedText))
                    {
                        var existingAudio = await _db.Audios
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x =>
                                x.TranslationId == existingTranslation.Id,
                                cancellationToken);

                        if (existingAudio != null &&
                            existingAudio.Status == "Success" &&
                            existingAudio.DurationSeconds > 0 &&
                            AudioFileExists(
                                webRoot, existingAudio.FilePath))
                        {
                            result[code] = "AudioAlreadyExists";
                            continue;
                        }
                    }

                    job.AttemptCount = 0;
                }

                if (job.Status == "Processing")
                {
                    var lastAttempt =
                        job.LastAttemptAt ?? job.CreatedAt;

                    if (now - lastAttempt <
                        TimeSpan.FromMinutes(15))
                    {
                        result[code] = "AlreadyProcessing";
                        continue;
                    }

                    job.Status = "Failed";
                    job.ErrorMessage = "Interrupted processing.";
                    job.CompletedAt = now;
                    await _db.SaveChangesAsync(cancellationToken);
                }

                if (automatic && job.Status == "Failed")
                {
                    if (job.AttemptCount >= 4)
                    {
                        result[code] = "RetryLimitReached";
                        continue;
                    }

                    var delayMinutes = job.AttemptCount switch
                    {
                        1 => 1,
                        2 => 5,
                        3 => 15,
                        _ => 0
                    };

                    var failedAt =
                        job.CompletedAt ??
                        job.LastAttemptAt ??
                        job.CreatedAt;

                    if (now < failedAt.AddMinutes(delayMinutes))
                    {
                        result[code] = "WaitingForRetry";
                        continue;
                    }
                }

                if (!automatic && job.AttemptCount >= 4)
                {
                    job.AttemptCount = 0;
                }

                jobId = job.Id;
                job.AttemptCount++;
                job.LastAttemptAt = now;
                job.Status = "Processing";
                job.ErrorMessage = null;
                job.CompletedAt = null;

                await _db.SaveChangesAsync(cancellationToken);

                var translation = await _db.Translations
                    .FirstOrDefaultAsync(
                        x => x.ShopSubmissionId == submissionId &&
                             x.LanguageId == language.Id,
                        cancellationToken);

                bool translationChanged = false;

                if (translation == null ||
                    translation.Status != "Success" ||
                    string.IsNullOrWhiteSpace(
                        translation.TranslatedText))
                {
                    var text = code == "vi"
                        ? sub.Description.Trim()
                        : await _translator.TranslateAsync(
                            sub.Description,
                            code,
                            cancellationToken);

                    if (string.IsNullOrWhiteSpace(text))
                        throw new InvalidOperationException(
                            "Bản dịch rỗng.");

                    if (translation == null)
                    {
                        translation = new Translation
                        {
                            ShopSubmissionId = submissionId,
                            LanguageId = language.Id
                        };

                        _db.Translations.Add(translation);
                    }

                    translation.TranslatedText = text.Trim();
                    translation.Status = "Success";
                    translationChanged = true;

                    await _db.SaveChangesAsync(cancellationToken);
                }

                var audio = await _db.Audios
                    .FirstOrDefaultAsync(
                        x => x.TranslationId == translation.Id,
                        cancellationToken);

                if (!translationChanged &&
                    audio != null &&
                    audio.Status == "Success" &&
                    audio.DurationSeconds > 0 &&
                    AudioFileExists(webRoot, audio.FilePath))
                {
                    job.Status = "Success";
                    job.CompletedAt = DateTime.UtcNow;
                    job.ErrorMessage = null;

                    await _db.SaveChangesAsync(cancellationToken);

                    result[code] = "AudioAlreadyExists";
                    continue;
                }

                // Use a new filename to preserve existing audio files.
                var fileName =
                    $"submission-{submissionId}-{code}-{Guid.NewGuid():N}.mp3";

                var outputPath = Path.Combine(
                    audioDirectory, fileName);

                var generated = await _tts.GenerateAsync(
                    translation.TranslatedText,
                    code,
                    outputPath,
                    cancellationToken);

                if (!File.Exists(outputPath) ||
                    new FileInfo(outputPath).Length == 0 ||
                    generated.DurationSeconds <= 0)
                {
                    throw new InvalidOperationException(
                        "File MP3 không hợp lệ.");
                }

                if (audio == null)
                {
                    audio = new Audio
                    {
                        TranslationId = translation.Id
                    };

                    _db.Audios.Add(audio);
                }

                audio.FilePath =
                    $"/audio/generated/{fileName}";

                audio.DurationSeconds =
                    generated.DurationSeconds;

                audio.Status = "Success";

                job.Status = "Success";
                job.ErrorMessage = null;
                job.CompletedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync(cancellationToken);

                result[code] = "AudioCreated";
            }
            catch (Exception ex)
            {
                _db.ChangeTracker.Clear();

                if (jobId.HasValue)
                {
                    var failedJob = await _db.ProcessingJobs
                        .FirstOrDefaultAsync(
                            x => x.Id == jobId.Value,
                            CancellationToken.None);

                    if (failedJob != null)
                    {
                        failedJob.Status = "Failed";
                        failedJob.ErrorMessage = ex.Message;
                        failedJob.CompletedAt = DateTime.UtcNow;

                        await _db.SaveChangesAsync(
                            CancellationToken.None);
                    }
                }

                result[code] = "Failed: " + ex.Message;

                if (cancellationToken.IsCancellationRequested)
                    throw;
            }
        }

        await NarrationPublicationService.TryPublishAsync(_db, submissionId, result, cancellationToken);
        return result;
    }

    private static bool AudioFileExists(
        string webRoot,
        string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return false;

        var path = relativePath.Replace('\\', '/');

        if (!path.StartsWith("/audio/",
                StringComparison.OrdinalIgnoreCase))
            return false;

        var fullPath = Path.GetFullPath(
            Path.Combine(webRoot, path.TrimStart('/')));

        var rootPrefix = webRoot.TrimEnd(
            Path.DirectorySeparatorChar) +
            Path.DirectorySeparatorChar;

        return fullPath.StartsWith(
                   rootPrefix,
                   StringComparison.OrdinalIgnoreCase) &&
               File.Exists(fullPath) &&
               new FileInfo(fullPath).Length > 0;
    }
}
