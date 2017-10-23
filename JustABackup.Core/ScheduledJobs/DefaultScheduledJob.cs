using JustABackup.Base;
using JustABackup.Database;
using JustABackup.Database.Entities;
using JustABackup.Database.Enum;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.ScheduledJobs
{
    public class DefaultScheduledJob : IJob
    {
        private DefaultContext databaseContext;

        public DefaultScheduledJob(DefaultContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            int jobId = int.Parse(context.JobDetail.Key.Name);

            BackupJob job = databaseContext.Jobs
                .Include(j => j.BackupProvider)
                .Include(j => j.BackupProvider.Provider)
                .Include(j => j.BackupProvider.Values)
                .ThenInclude(x => x.Property)
                .Include(j => j.StorageProvider)
                .Include(j => j.StorageProvider.Provider)
                .Include(j => j.StorageProvider.Values)
                .ThenInclude(x => x.Property)
                .FirstOrDefault(j => j.ID == jobId);

            BackupJobHistory history = new BackupJobHistory();
            history.Started = DateTime.Now;

            job.History.Add(history);
            await databaseContext.SaveChangesAsync();

            Type backupProviderType = Type.GetType(job.BackupProvider.Provider.Namespace);
            IBackupProvider backupProvider = Activator.CreateInstance(backupProviderType) as IBackupProvider;
            foreach (var property in job.BackupProvider.Values)
            {
                PropertyInfo propertyInfo = backupProviderType.GetProperty(property.Property.TypeName);
                object originalValueType = Convert.ChangeType(property.Value, propertyInfo.PropertyType);

                propertyInfo.SetValue(backupProvider, originalValueType);
            }

            Type storageProviderType = Type.GetType(job.StorageProvider.Provider.Namespace);
            IStorageProvider storageProvider = Activator.CreateInstance(storageProviderType) as IStorageProvider;
            foreach (var property in job.StorageProvider.Values)
            {
                PropertyInfo propertyInfo = storageProviderType.GetProperty(property.Property.TypeName);
                object originalValueType = Convert.ChangeType(property.Value, propertyInfo.PropertyType);

                propertyInfo.SetValue(storageProvider, originalValueType);
            }

            var items = await backupProvider.GetItems();
            foreach (var item in items)
            {
                using (var stream = await backupProvider.OpenRead(item))
                {
                    await storageProvider.StoreItem(item, stream);
                }
            }

            history.Completed = DateTime.Now;
            history.Message = $"{items.Count()} files were copied.";
            history.Status = ExitCode.Success;

            await databaseContext.SaveChangesAsync();
        }
    }
}
