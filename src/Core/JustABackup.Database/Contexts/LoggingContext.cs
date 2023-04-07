using JustABackup.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace JustABackup.Database.Contexts
{
    public abstract class LoggingContext : DbContext
    {
        public DbSet<LogEntry> Logs { get; set; } = null!;

        protected LoggingContext(DbContextOptions<LoggingContext> options) : base(options)
        {
        }
    }
}
