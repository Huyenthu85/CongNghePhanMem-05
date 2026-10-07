using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task Login_ThanhCong_TraToken()
    {
        var user = new User
        {
            Id = 1,
            Username = "owner1",
            PasswordHash = "hash",
            Role = Role.Owner,
            Active = true
        };

        var users = new Mock<IUserRepository>();
        users.Setup(r => r.FindByUsernameAsync("owner1")).ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher<User>>();
        hasher.Setup(h => h.VerifyHashedPassword(user, "hash", "123"))
              .Returns(PasswordVerificationResult.Success);

        var tokens = new Mock<ITokenService>();
        tokens.Setup(t => t.Generate(user)).Returns("jwt-token");

        var svc = new AuthService(users.Object, tokens.Object, hasher.Object);

        var token = await svc.LoginAsync("owner1", "123");

        Assert.Equal("jwt-token", token);
    }

    [Fact]
    public async Task Login_SaiMatKhau_NemAuthException()
    {
        var user = new User { Id = 1, Username = "owner1", PasswordHash = "hash", Active = true };

        var users = new Mock<IUserRepository>();
        users.Setup(r => r.FindByUsernameAsync("owner1")).ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher<User>>();
        hasher.Setup(h => h.VerifyHashedPassword(user, "hash", "sai"))
              .Returns(PasswordVerificationResult.Failed);

        var svc = new AuthService(users.Object, Mock.Of<ITokenService>(), hasher.Object);

        await Assert.ThrowsAsync<AuthException>(() => svc.LoginAsync("owner1", "sai"));
    }

    [Fact]
    public async Task Login_TaiKhoanKhongTonTai_NemAuthException()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(r => r.FindByUsernameAsync("ghost")).ReturnsAsync((User?)null);

        var svc = new AuthService(users.Object, Mock.Of<ITokenService>(),
            Mock.Of<IPasswordHasher<User>>());

        await Assert.ThrowsAsync<AuthException>(() => svc.LoginAsync("ghost", "any"));
    }
}