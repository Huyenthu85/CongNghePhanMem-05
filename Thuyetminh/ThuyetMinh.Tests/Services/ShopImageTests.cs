using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using ThuyetMinh.Api.Controllers;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
using Xunit;

namespace ThuyetMinh.Tests.Services;

public class ShopImageTests
{
    [Fact]
    public void ParseImages_KeepsLegacyAndSafeUploadedPaths()
    {
        const string url = "/images/shop-uploads/1/0123456789abcdef0123456789abcdef.png";
        Assert.Equal(new[] { "https://example.com/old.jpg", url },
            ShopService.ParseImageUrls($"https://example.com/old.jpg\n{url}\n/images/shop-uploads/1/../../hidden.png\njavascript:alert(1)"));
    }

    [Fact]
    public async Task UploadedImage_IsPrivateUntilPublished()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var owner = new User { Username = "image-owner", PasswordHash = "test", Role = Role.Owner };
        db.Users.Add(owner);
        await db.SaveChangesAsync();
        var shop = new Shop { OwnerId = owner.Id, Name = "Shop", Address = "Address" };
        db.Shops.Add(shop);
        await db.SaveChangesAsync();
        const string name = "0123456789abcdef0123456789abcdef.png";
        var submission = new ShopSubmission
        {
            ShopId = shop.Id, SubmittedById = owner.Id, Name = shop.Name, Address = shop.Address,
            ImageUrlsRaw = $"/images/shop-uploads/{shop.Id}/{name}", Status = SubmissionStatus.Pending
        };
        db.ShopSubmissions.Add(submission);
        await db.SaveChangesAsync();

        var root = Directory.CreateTempSubdirectory("shop-image-test-");
        try
        {
            var folder = Directory.CreateDirectory(Path.Combine(root.FullName, "App_Data", "shop-images", shop.Id.ToString()));
            await File.WriteAllBytesAsync(Path.Combine(folder.FullName, name), new byte[] { 137, 80, 78, 71 });
            var environment = Mock.Of<IWebHostEnvironment>(e => e.ContentRootPath == root.FullName);
            var controller = new ShopImageController(db, environment)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
            ClaimsPrincipal Principal(string role, long id) => new(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, id.ToString())
            }, "test"));

            Assert.IsType<NotFoundResult>(await controller.Get(shop.Id, name, default));
            controller.HttpContext.User = Principal("Owner", owner.Id + 1);
            Assert.IsType<NotFoundResult>(await controller.Get(shop.Id, name, default));
            controller.HttpContext.User = Principal("Owner", owner.Id);
            Assert.IsType<PhysicalFileResult>(await controller.Get(shop.Id, name, default));
            controller.HttpContext.User = Principal("Admin", 1);
            Assert.IsType<PhysicalFileResult>(await controller.Get(shop.Id, name, default));
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            submission.Status = SubmissionStatus.Approved;
            shop.PublishStatus = PublishStatus.Approved;
            shop.PublishedVersionId = submission.Id;
            await db.SaveChangesAsync();
            Assert.IsType<PhysicalFileResult>(await controller.Get(shop.Id, name, default));
            shop.PublishStatus = PublishStatus.Draft;
            await db.SaveChangesAsync();
            Assert.IsType<NotFoundResult>(await controller.Get(shop.Id, name, default));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
