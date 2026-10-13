using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Business.Services;

public interface ITokenService
{
    string Generate(User user);
    void Invalidate(string token);
    bool IsBlacklisted(string token);
}

public class JwtTokenService : ITokenService
{
    private readonly string _secret;
    private static readonly HashSet<string> _blacklist = new();
    private static readonly object _lock = new();

    public JwtTokenService(IConfiguration cfg)
        => _secret = cfg["Jwt:Secret"] ?? "dev-secret-change-me";

    public string Generate(User user)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secret);
        var token = new JwtSecurityToken(
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            },
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));
        return handler.WriteToken(token);
    }

    public void Invalidate(string token)
    {
        lock (_lock) _blacklist.Add(token);
    }

    public bool IsBlacklisted(string token)
    {
        lock (_lock) return _blacklist.Contains(token);
    }
}