using JustABackup.Database.Contexts;
using JustABackup.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace JustABackup.Database.SQLite
{
    public class DefaultSQLiteContext : DefaultContext
    {
        public DbSet<BackupJob> Jobs { get; set; } = null!;

        public DbSet<BackupJobHistory> JobHistory { get; set; } = null!;

        public DbSet<Provider> Providers { get; set; } = null!;

        public DbSet<ProviderInstance> ProviderInstances { get; set; } = null!;

        public DbSet<AuthenticatedSession> AuthenticatedSessions { get; set; } = null!;

        public DbSet<Passphrase> Passphrase { get; set; } = null!;

        public DefaultSQLiteContext(DbContextOptions<DefaultSQLiteContext> options) : base(options)
        {
        }
    }
}
