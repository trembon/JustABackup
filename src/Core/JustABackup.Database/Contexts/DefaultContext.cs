using JustABackup.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace JustABackup.Database.Contexts
{
    public abstract class DefaultContext : DbContext
    {
        public DbSet<BackupJob> Jobs { get; set; } = null!;

        public DbSet<BackupJobHistory> JobHistory { get; set; } = null!;

        public DbSet<Provider> Providers { get; set; } = null!;

        public DbSet<ProviderInstance> ProviderInstances { get; set; } = null!;

        public DbSet<AuthenticatedSession> AuthenticatedSessions { get; set; } = null!;

        public DbSet<Passphrase> Passphrase { get; set; } = null!;

        protected DefaultContext(DbContextOptions options) : base(options)
        {
        }
    }
}
