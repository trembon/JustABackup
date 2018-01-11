using JustABackup.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Database.Repositories
{
    public interface IBackupJobRepository
    {
        Task<BackupJob> Get(int id);

        Task<IEnumerable<BackupJob>> Get();

        Task<IEnumerable<BackupJobHistory>> GetHistory(int limit);

        Task<int> AddOrUpdate(int? id, string name, IEnumerable<ProviderInstance> providerInstances);
    }

    public class BackupJobRepository : IBackupJobRepository
    {
        private DefaultContext context;

        public BackupJobRepository(DefaultContext context)
        {
            this.context = context;
        }

        public async Task<int> AddOrUpdate(int? id, string name, IEnumerable<ProviderInstance> providerInstances)
        {
            BackupJob job = null;
            if (id != null && id > 0)
            {
                job = await context.Jobs.Include(j => j.Providers).FirstOrDefaultAsync(j => j.ID == id);
                job.HasChangedModel = false;
                job.Providers.Clear();
            }
            else
            {
                job = new BackupJob();
                context.Jobs.Add(job);
            }

            job.Name = name;
            job.Providers.AddRange(providerInstances);

            await context.SaveChangesAsync();

            return job.ID;
        }

        public async Task<BackupJob> Get(int id)
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

        public async Task<IEnumerable<BackupJob>> Get()
        {
            return await context
                .Jobs
                .OrderBy(j => j.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<BackupJobHistory>> GetHistory(int limit)
        {
            return await context
                .JobHistory
                .OrderByDescending(jh => jh.Started)
                .Take(limit)
                .ToListAsync();
        }
    }
}
