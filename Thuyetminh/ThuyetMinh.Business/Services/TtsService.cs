
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ThuyetMinh.Business.Services;

public interface ITtsService
{
    Task<TtsResult> GenerateAsync(
        string text,
        string language,
        string outputPath,
        CancellationToken cancellationToken = default);
}

public class TtsResult
{
    public string FilePath { get; set; } = "";
    public int DurationSeconds { get; set; }
}

public class TtsService : ITtsService
{
    public async Task<TtsResult> GenerateAsync(
        string text,
        string language,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Noi dung thuyet minh khong duoc de trong.");

        if (language is not ("vi" or "en" or "zh" or "km"))
            throw new ArgumentException("Ngon ngu khong duoc ho tro.");

        if (string.IsNullOrWhiteSpace(outputPath) ||
            Path.GetExtension(outputPath).ToLowerInvariant() != ".mp3")
            throw new ArgumentException("Duong dan MP3 khong hop le.");

        var script = Path.Combine(
            AppContext.BaseDirectory,
            "scripts",
            "tts.py");

        if (!File.Exists(script))
            throw new FileNotFoundException(
                "Khong tim thay file tts.py.", script);

        var info = new ProcessStartInfo
        {
            FileName = "py",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = Encoding.UTF8,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        info.ArgumentList.Add("-X");
        info.ArgumentList.Add("utf8");
        info.ArgumentList.Add(script);
        info.ArgumentList.Add(language);
        info.ArgumentList.Add(Path.GetFullPath(outputPath));

        using var process = new Process { StartInfo = info };

        using var timeout = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(3));

        if (!process.Start())
            throw new InvalidOperationException(
                "Khong the khoi dong dich vu TTS.");

        try
        {
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.StandardInput.WriteAsync(
                text.AsMemory(), timeout.Token);
            process.StandardInput.Close();

            await process.WaitForExitAsync(timeout.Token);

            var output = (await outputTask).Trim();
            var error = (await errorTask).Trim();

            if (process.ExitCode != 0)
                throw new InvalidOperationException(
                    "Tao audio that bai: " + error);

            var result = JsonSerializer.Deserialize<TtsResult>(
                output,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result == null ||
                result.DurationSeconds <= 0 ||
                !File.Exists(result.FilePath))
                throw new InvalidOperationException(
                    "Ket qua tao MP3 khong hop le.");

            return result;
        }
        catch
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);

            throw;
        }
    }
}
