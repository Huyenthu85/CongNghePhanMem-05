using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data;

namespace ThuyetMinh.Api.Services;

public static class AccountSessionValidator
{
    public static Task<bool> IsValidAsync(AppDbContext db, ClaimsPrincipal? principal,
        CancellationToken cancellationToken = default)
    {
        if (!long.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ||
            !int.TryParse(principal?.FindFirstValue("tm_session_version"), out var version))
            return Task.FromResult(false);

        return db.Users.AsNoTracking().AnyAsync(u => u.Id == id && u.Active &&
            u.SessionVersion == version, cancellationToken);
    }
}
