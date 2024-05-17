using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ParserWebApp.Data
{
    public class LogDbContext : DbContext
    {
        public DbSet<LogEntry> LogEntries { get; set; }

        public LogDbContext(DbContextOptions<LogDbContext> options) : base(options)
        {
        }

        public async Task CleanDatabaseAsync()
        {
            await Database.ExecuteSqlRawAsync("DELETE FROM [LogEntries]");
        }
    }
}
