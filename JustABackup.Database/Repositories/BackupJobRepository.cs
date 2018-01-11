using JustABackup.Database.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Database.Repositories
{
    public interface IBackupJobRepository
    {
        Task<BackupJob> GetBackupJob(long id);
    }

    public class BackupJobRepository : IBackupJobRepository
    {
        private DefaultContext context;

        public BackupJobRepository(DefaultContext context)
        {
            this.context = context;
        }

        public async Task<BackupJob> GetBackupJob(long id)
        {
            return await context
                .Jobs
                .Include(j => j.Providers)
                .ThenInclude(x => x.Provider)
                .Include(j => j.Providers)
                .ThenInclude(x => x.Values)
                .ThenInclude(x => x.Property)
                .FirstOrDefaultAsync(j => j.ID == id);
        }
    }
}
