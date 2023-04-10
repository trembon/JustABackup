using JustABackup.Database.Contexts;
using Microsoft.EntityFrameworkCore;

namespace JustABackup.Database.SQLite
{
    public class LoggingSQLiteContext : LoggingContext
    {
        public LoggingSQLiteContext(DbContextOptions<LoggingContext> options) : base(options)
        {
        }
    }
}
