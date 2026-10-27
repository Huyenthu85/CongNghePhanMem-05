
using System.Security.Cryptography;
using System.Text;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;
namespace ThuyetMinh.Business.Services;

public interface IAnonymousVisitorService
{
    Task<(AnonymousVisitor Visitor, string? NewToken)> GetOrCreateAsync(string? token);
    Task<AnonymousVisitor?> FindExistingAsync(string? token);
}
public class AnonymousVisitorService : IAnonymousVisitorService
{
    private readonly IAnonymousVisitorRepository _repository;
    public AnonymousVisitorService(IAnonymousVisitorRepository repository)
    {
        _repository = repository;
    }
    public async Task<(AnonymousVisitor Visitor, string? NewToken)> GetOrCreateAsync(string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            string tokenHash = ComputeHash(token);
            var visitor = await _repository.GetByTokenHashAsync(tokenHash);
            if (visitor != null && visitor.IsActive)
            {
                await _repository.UpdateLastSeenAsync(visitor);
                return (visitor, null);
            }
        }
        string newToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        string newTokenHash = ComputeHash(newToken);
        var newVisitor = await _repository.CreateAsync(newTokenHash);
        return (newVisitor, newToken);
    }

    public async Task<AnonymousVisitor?> FindExistingAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token) ||
            token.Length != 64 ||
            !token.All(Uri.IsHexDigit))
        {
            return null;
        }
        string tokenHash = ComputeHash(token);
        var visitor = await _repository.GetByTokenHashAsync(tokenHash);
        return visitor?.IsActive == true ? visitor : null;
    }

    private static string ComputeHash(string token)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
