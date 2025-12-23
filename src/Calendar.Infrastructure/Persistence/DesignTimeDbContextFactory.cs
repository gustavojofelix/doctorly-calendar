using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Calendar.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var opt = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=calendar.db")
            .Options;

        return new AppDbContext(opt);
    }
}
