using JustABackup.Database.Contexts;
using JustABackup.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace JustABackup.Database.SQLite
{
    public class LoggingSQLiteContext : LoggingContext
    {
        public DbSet<LogEntry> Logs { get; set; } = null!;

        public LoggingSQLiteContext(DbContextOptions<LoggingContext> options) : base(options)
        {
        }
    }
}
