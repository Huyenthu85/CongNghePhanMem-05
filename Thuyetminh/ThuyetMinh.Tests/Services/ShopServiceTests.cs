using Moq;
using Xunit;
using ThuyetMinh.Business.Dtos;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Tests.Services;

public class ShopServiceTests
{
    [Fact]
    public async Task Owner_KhongTruyCapShopCuaNguoiKhac()
    {
        var shops = new Mock<IShopRepository>();
        shops.Setup(r => r.FindByIdAndOwnerIdAsync(10, 99))
             .ReturnsAsync((Shop?)null);

        var svc = new ShopService(shops.Object, Mock.Of<IShopSubmissionRepository>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => svc.GetMyShopAsync(10, 99));
    }

    [Fact]
    public async Task Owner_TruyCapShopCuaMinh_ThanhCong()
    {
        var shop = new Shop { Id = 10, OwnerId = 99, Name = "Quán A" };
        var shops = new Mock<IShopRepository>();
        shops.Setup(r => r.FindByIdAndOwnerIdAsync(10, 99)).ReturnsAsync(shop);

        var svc = new ShopService(shops.Object, Mock.Of<IShopSubmissionRepository>());

        var result = await svc.GetMyShopAsync(10, 99);

        Assert.Equal("Quán A", result.Name);
    }

    [Fact]
    public async Task BanChinhSuaChoDuyet_KhongGhiDeBanDangPhucVu()
    {
        var shop = new Shop
        {
            Id = 5,
            OwnerId = 1,
            Name = "Tên cũ",
            PublishedVersionId = 50,
            PublishStatus = PublishStatus.Approved
        };

        var shops = new Mock<IShopRepository>();
        shops.Setup(r => r.FindByIdAndOwnerIdAsync(5, 1)).ReturnsAsync(shop);

        var subs = new Mock<IShopSubmissionRepository>();
        subs.Setup(r => r.AddAsync(It.IsAny<ShopSubmission>()))
            .ReturnsAsync((ShopSubmission s) => s);

        var svc = new ShopService(shops.Object, subs.Object);

        var req = new ShopSubmissionRequest
        {
            Name = "Tên mới",
            Address = "Địa chỉ mới",
            Latitude = 21.1,
            Longitude = 105.9,
            ActivationRadiusMeters = 100,
            Submit = true
        };

        var sub = await svc.CreateSubmissionAsync(5, 1, req);

        Assert.Equal(SubmissionStatus.Pending, sub.Status);
        Assert.Equal("Tên cũ", shop.Name);
        Assert.Equal(50, shop.PublishedVersionId);
        shops.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task BaiChuaDuyet_KhongCoNoiDungCongBo()
    {
        var subs = new Mock<IShopSubmissionRepository>();
        subs.Setup(r => r.FindLatestApprovedByShopIdAsync(5))
            .ReturnsAsync((ShopSubmission?)null);

        var svc = new ShopService(Mock.Of<IShopRepository>(), subs.Object);

        var content = await svc.GetPublishedContentAsync(5);

        Assert.Null(content);
    }

    [Fact]
    public async Task BaiDaDuyet_TraVeNoiDungCongBo()
    {
        var approved = new ShopSubmission
        {
            Id = 100,
            ShopId = 5,
            Name = "Quán đã duyệt",
            Address = "Hà Nội",
            Latitude = 21,
            Longitude = 105.8,
            ActivationRadiusMeters = 50,
            Status = SubmissionStatus.Approved
        };

        var subs = new Mock<IShopSubmissionRepository>();
        subs.Setup(r => r.FindLatestApprovedByShopIdAsync(5)).ReturnsAsync(approved);

        var svc = new ShopService(Mock.Of<IShopRepository>(), subs.Object);

        var content = await svc.GetPublishedContentAsync(5);

        Assert.NotNull(content);
        Assert.Equal("Quán đã duyệt", content!.Name);
        Assert.Equal(100, content.VersionId);
    }
}