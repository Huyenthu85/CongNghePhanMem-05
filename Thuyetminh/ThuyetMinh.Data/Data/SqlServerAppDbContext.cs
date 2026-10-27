using Microsoft.EntityFrameworkCore;

namespace ThuyetMinh.Data;

public class SqlServerAppDbContext : AppDbContext
{
    public SqlServerAppDbContext(
        DbContextOptions<SqlServerAppDbContext> options
    ) : base(options)
    {
    }
}