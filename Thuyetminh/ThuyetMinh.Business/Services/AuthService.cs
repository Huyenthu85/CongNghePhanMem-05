using Microsoft.AspNetCore.Identity;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;

namespace ThuyetMinh.Business.Services;

public interface IAuthService
{
    Task<string> LoginAsync(string username, string password);
    void Logout(string token);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;
    private readonly IPasswordHasher<User> _hasher;

    public AuthService(IUserRepository users, ITokenService tokens, IPasswordHasher<User> hasher)
    {
        _users = users;
        _tokens = tokens;
        _hasher = hasher;
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        var u = await _users.FindByUsernameAsync(username)
            ?? throw new AuthException("Sai tài khoản hoặc mật khẩu");

        if (!u.Active) throw new AuthException("Tài khoản đã bị khóa");

        var result = _hasher.VerifyHashedPassword(u, u.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
            throw new AuthException("Sai tài khoản hoặc mật khẩu");

        return _tokens.Generate(u);
    }

    public void Logout(string token) => _tokens.Invalidate(token);
}