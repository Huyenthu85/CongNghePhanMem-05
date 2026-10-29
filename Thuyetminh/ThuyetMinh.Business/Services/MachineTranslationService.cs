
using System.Diagnostics;
using System.Text;

namespace ThuyetMinh.Business.Services;

public interface IMachineTranslationService
{
    Task<string> TranslateAsync(
        string text,
        string targetLanguage,
        CancellationToken cancellationToken = default);
}

public class MachineTranslationService : IMachineTranslationService
{
    public async Task<string> TranslateAsync(
        string text,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Noi dung khong duoc de trong.");

        if (targetLanguage == "vi")
            return text.Trim();

        if (targetLanguage != "en" &&
            targetLanguage != "zh" &&
            targetLanguage != "km")
            throw new ArgumentException("Ngon ngu khong duoc ho tro.");

        var script = Path.Combine(
            AppContext.BaseDirectory,
            "scripts",
            "translate.py");

        if (!File.Exists(script))
            throw new FileNotFoundException(
                "Khong tim thay chuong trinh dich.", script);

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
        info.ArgumentList.Add(targetLanguage);

        using var process = new Process { StartInfo = info };

        using var timeout = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(45));

        if (!process.Start())
            throw new InvalidOperationException(
                "Khong the khoi dong dich vu dich.");

        try
        {
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.StandardInput.WriteAsync(
                text.AsMemory(), timeout.Token);
            process.StandardInput.Close();

            await process.WaitForExitAsync(timeout.Token);

            var result = (await outputTask).Trim();
            var error = (await errorTask).Trim();

            if (process.ExitCode != 0)
                throw new InvalidOperationException(
                    "Dich tu dong that bai: " + error);

            if (string.IsNullOrWhiteSpace(result))
                throw new InvalidOperationException(
                    "Dich vu khong tra ve noi dung.");

            return result;
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);

            throw;
        }
        catch
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);

            throw;
        }
    }
}
