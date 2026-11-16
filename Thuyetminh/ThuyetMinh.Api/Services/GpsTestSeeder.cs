using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Api.Services;

public static class GpsTestSeeder
{
    public static void Run(WebApplication app)
    {
        if (!app.Environment.IsDevelopment() ||
            !app.Configuration.GetValue<bool>("GpsTest:Enabled"))
            return;

        var culture = CultureInfo.InvariantCulture;
        var style = NumberStyles.Float;

        if (!double.TryParse(app.Configuration["GpsTest:Latitude"],
                style, culture, out double lat) ||
            !double.TryParse(app.Configuration["GpsTest:Longitude"],
                style, culture, out double lon))
            throw new InvalidOperationException("Missing GPS test coordinates.");

        double radius = app.Configuration.GetValue<double>(
            "GpsTest:RadiusMeters", 200);

        if (!double.IsFinite(lat) || !double.IsFinite(lon) ||
            !double.IsFinite(radius) ||
            Math.Abs(lat) > 90 || Math.Abs(lon) > 180 ||
            radius < 1 || radius > 5000)
            throw new InvalidOperationException("Invalid GPS test values.");

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var owner = db.Users.FirstOrDefault(x =>
            x.Role == Role.Owner && x.Active)
            ?? throw new InvalidOperationException("Missing Owner.");

        var admin = db.Users.FirstOrDefault(x =>
            x.Role == Role.Admin && x.Active);

        var vi = db.Languages.FirstOrDefault(x => x.Code == "vi")
            ?? throw new InvalidOperationException("Missing vi language.");

        const string name = "Điểm kiểm thử GPS";
        const string address = "Vị trí kiểm thử nội bộ";
        const string description =
            "Chào mừng đến với điểm kiểm thử GPS. " +
            "Hệ thống đang kiểm tra vị trí và thời lượng nghe.";

        using var tx = db.Database.BeginTransaction();

        var shop = db.Shops.SingleOrDefault(x =>
            x.Name == name && x.Address == address);

        if (shop == null)
        {
            shop = new Shop
            {
                OwnerId = owner.Id,
                Name = name,
                Address = address,
                Latitude = lat,
                Longitude = lon,
                ActivationRadiusMeters = radius,
                PublishStatus = PublishStatus.Approved
            };

            db.Shops.Add(shop);
            db.SaveChanges();
        }
        else if (Math.Abs(shop.Latitude - lat) > 0.0000001 ||
                 Math.Abs(shop.Longitude - lon) > 0.0000001)
        {
            throw new InvalidOperationException(
                "Existing GPS test location has different coordinates.");
        }

        var version = db.ShopSubmissions
            .Where(x => x.ShopId == shop.Id &&
                        x.Status == SubmissionStatus.Approved)
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();

        if (version == null)
        {
            version = new ShopSubmission
            {
                ShopId = shop.Id,
                SubmittedById = owner.Id,
                ReviewedById = admin?.Id,
                ReviewedAt = DateTime.UtcNow,
                Name = name,
                Address = address,
                Latitude = lat,
                Longitude = lon,
                ActivationRadiusMeters = radius,
                Description = description,
                Status = SubmissionStatus.Approved
            };

            db.ShopSubmissions.Add(version);
            db.SaveChanges();
        }

        shop.PublishedVersionId = version.Id;
        shop.PublishStatus = PublishStatus.Approved;

        var translation = db.Translations.FirstOrDefault(x =>
            x.ShopSubmissionId == version.Id &&
            x.LanguageId == vi.Id);

        if (translation == null)
        {
            translation = new Translation
            {
                ShopSubmissionId = version.Id,
                LanguageId = vi.Id,
                TranslatedText = description,
                Status = "Success"
            };

            db.Translations.Add(translation);
            db.SaveChanges();
        }

        var webRoot = app.Environment.WebRootPath ??
            Path.Combine(app.Environment.ContentRootPath, "wwwroot");

        var audioFile = Path.Combine(webRoot, "audio", "quan-mau.mp3");

        if (File.Exists(audioFile) &&
            !db.Audios.Any(x => x.TranslationId == translation.Id))
        {
            db.Audios.Add(new Audio
            {
                TranslationId = translation.Id,
                FilePath = "/audio/quan-mau.mp3",
                DurationSeconds = 5,
                Status = "Success"
            });
        }

        db.SaveChanges();
        tx.Commit();

        Console.WriteLine(
            $"GPS TEST SITE READY | ShopId={shop.Id} | Radius={shop.ActivationRadiusMeters}m");
    }
}