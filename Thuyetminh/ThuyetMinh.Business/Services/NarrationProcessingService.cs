
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Business.Services;

public class NarrationProcessingService
{
    private readonly AppDbContext _db;
    private readonly TranslationService _translations;
    private readonly IMachineTranslationService _translator;
    private readonly AudioService _audios;
    private readonly ITtsService _tts;

    public NarrationProcessingService(
        AppDbContext db,
        TranslationService translations,
        IMachineTranslationService translator,
        AudioService audios,
        ITtsService tts)
    {
        _db = db;
        _translations = translations;
        _translator = translator;
        _audios = audios;
        _tts = tts;
    }

    public async Task<Dictionary<string, string>> ProcessAsync(
        long submissionId,
        CancellationToken cancellationToken = default)
    {
        var sub = await _db.ShopSubmissions
            .Include(x => x.Shop)
            .FirstOrDefaultAsync(x => x.Id == submissionId, cancellationToken);

        if (sub == null)
            throw new KeyNotFoundException("Khong tim thay bai gui.");

        if (sub.Status != SubmissionStatus.Approved ||
            sub.Shop.PublishedVersionId != sub.Id ||
            sub.Shop.PublishStatus != PublishStatus.Approved)
            throw new InvalidOperationException("Bai gui chua duoc xuat ban.");

        if (string.IsNullOrWhiteSpace(sub.Description))
            throw new InvalidOperationException("Bai gui chua co noi dung thuyet minh.");

        var result = new Dictionary<string, string>();
        var codes = new[] { "vi", "en", "zh", "km" };

        foreach (var code in codes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var language = await _db.Languages
                    .FirstOrDefaultAsync(
                        x => x.Code == code && x.IsActive,
                        cancellationToken);

                if (language == null)
                {
                    result[code] = "Failed: Ngon ngu khong hoat dong";
                    continue;
                }

                var translation = _translations.GetTranslation(
                    submissionId, language.Id);

                if (translation == null)
                {
                    var text = code == "vi"
                        ? sub.Description.Trim()
                        : await _translator.TranslateAsync(
                            sub.Description, code, cancellationToken);

                    if (string.IsNullOrWhiteSpace(text))
                        throw new InvalidOperationException("Ban dich rong.");

                    translation = _translations.CreateTranslation(
                        submissionId, language.Id, text);
                }

                if (translation.Status != "Success")
                {
                    result[code] = "Failed: Ban dich chua thanh cong";
                    continue;
                }

                var audio = _audios.GetAudioByTranslationId(
                    translation.Id);

                if (audio != null)
                {
                    result[code] = audio.Status == "Success"
                        ? "AudioAlreadyExists"
                        : "Failed: Audio da ton tai nhung chua thanh cong";
                    continue;
                }

                var webRoot = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot");

                var audioDirectory = Path.Combine(
                    webRoot, "audio", "generated");

                Directory.CreateDirectory(audioDirectory);

                var fileName = $"submission-{submissionId}-{code}.mp3";
                var outputPath = Path.Combine(audioDirectory, fileName);

                if (File.Exists(outputPath))
                    throw new InvalidOperationException(
                        "File MP3 da ton tai nhung chua co ban ghi Audio. Can kiem tra truoc khi tao lai.");

                var generated = await _tts.GenerateAsync(
                    translation.TranslatedText,
                    code,
                    outputPath,
                    cancellationToken);

                var publicPath = $"/audio/generated/{fileName}";

                _audios.CreateAudio(
                    translation.Id,
                    publicPath,
                    generated.DurationSeconds);

                result[code] = "AudioCreated";
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                result[code] = "Failed: " + ex.Message;
            }
        }

        return result;
    }
}
