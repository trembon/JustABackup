using JustABackup.Base;
using JustABackup.Core.Services;
using JustABackup.Database;
using JustABackup.Database.Entities;
using JustABackup.Database.Enum;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.ScheduledJobs
{
    public class DefaultScheduledJob : IJob
    {
        private class TransformBackupItem
        {
            public MappedBackupItem MappedBackupItem { get; set; }

            public Func<Stream, Task> Execute { get; set; }

            public int ID { get; set; }
        }

        private DefaultContext databaseContext;

        private IProviderMappingService providerMappingService;

        public DefaultScheduledJob(DefaultContext databaseContext, IProviderMappingService providerMappingService)
        {
            this.databaseContext = databaseContext;
            this.providerMappingService = providerMappingService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            int jobId = int.Parse(context.JobDetail.Key.Name);

            BackupJob job = databaseContext.Jobs
                .Include(j => j.Providers)
                .ThenInclude(x => x.Provider)
                .Include(j => j.Providers)
                .ThenInclude(x => x.Values)
                .ThenInclude(x => x.Property)
                .FirstOrDefault(j => j.ID == jobId);

            BackupJobHistory history = new BackupJobHistory();
            history.Started = DateTime.Now;

            job.History.Add(history);
            await databaseContext.SaveChangesAsync();

            var providers = job.Providers.OrderBy(p => p.Order);

            IBackupProvider backupProvider = await providerMappingService.CreateProvider<IBackupProvider>(providers.FirstOrDefault());
            IStorageProvider storageProvider = await providerMappingService.CreateProvider<IStorageProvider>(providers.LastOrDefault());
            
            List<ITransformProvider> transformProviders = new List<ITransformProvider>();
            foreach (var tp in providers.Where(p => p.Provider.Type == ProviderType.Transform))
                transformProviders.Add(await providerMappingService.CreateProvider<ITransformProvider>(tp));

            var items = await backupProvider.GetItems();

            List<List<TransformBackupItem>> transformExecuteList = new List<List<TransformBackupItem>>(transformProviders.Count());

            Dictionary<ITransformProvider, IEnumerable<MappedBackupItem>> transformers = new Dictionary<ITransformProvider, IEnumerable<MappedBackupItem>>(transformProviders.Count());
            for (int i = 0; i < transformProviders.Count(); i++)
            {
                if (i > 0)
                {
                    var mappedItems = await transformProviders[i].MapInput(transformers.Last().Value.Select(x => x.Output));
                    transformers.Add(transformProviders[i], mappedItems);

                    List<TransformBackupItem> subTransformExecuteList = new List<TransformBackupItem>();
                    foreach (var mappedItem in mappedItems)
                    {
                        int currentMappedIndex = i;
                        Func<Stream, Task> action = async (stream) =>
                        {
                            Dictionary<BackupItem, Stream> dictionary = new Dictionary<BackupItem, Stream>();
                            foreach (var backupItem in mappedItem.Input)
                            {
                                MemoryStream ms = new MemoryStream();
                                var transformBackupItem = transformExecuteList[currentMappedIndex - 1].FirstOrDefault(x => x.MappedBackupItem.Output == backupItem);
                                await transformBackupItem.Execute(ms);
                                ms.Seek(0, SeekOrigin.Begin);
                                dictionary.Add(backupItem, ms);
                            }

                            await transformProviders[currentMappedIndex].TransformItem(mappedItem.Output, stream, dictionary);
                        };

                        subTransformExecuteList.Add(new TransformBackupItem { MappedBackupItem = mappedItem, Execute = action });
                    }
                    transformExecuteList.Add(subTransformExecuteList);
                }
                else
                {
                    var mappedItems = await transformProviders[i].MapInput(items);
                    transformers.Add(transformProviders[i], mappedItems);

                    List<TransformBackupItem> subTransformExecuteList = new List<TransformBackupItem>();
                    foreach (var mappedItem in mappedItems)
                    {
                        int currentMappedIndex = i;
                        Func<Stream, Task> action = async (stream) =>
                        {
                            Dictionary<BackupItem, Stream> dictionary = new Dictionary<BackupItem, Stream>();
                            foreach (var backupItem in mappedItem.Input)
                                dictionary.Add(backupItem, await backupProvider.OpenRead(backupItem));

                            await transformProviders[currentMappedIndex].TransformItem(mappedItem.Output, stream, dictionary);
                        };

                        subTransformExecuteList.Add(new TransformBackupItem { MappedBackupItem = mappedItem, Execute = action });
                    }
                    transformExecuteList.Add(subTransformExecuteList);
                }
            }

            if (transformProviders.Count() > 0)
            {
                foreach(var mappedItem in transformExecuteList.Last())
                {
                    using(MemoryStream ms = new MemoryStream())
                    {
                        await mappedItem.Execute(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        await storageProvider.StoreItem(mappedItem.MappedBackupItem.Output, ms);
                    }
                }
            }
            else
            {
                foreach (var item in items)
                {
                    using (var stream = await backupProvider.OpenRead(item))
                    {
                        await storageProvider.StoreItem(item, stream);
                    }
                }
            }

            history.Completed = DateTime.Now;
            history.Message = $"{items.Count()} files were copied.";
            history.Status = ExitCode.Success;

            await databaseContext.SaveChangesAsync();
        }
    }
}
