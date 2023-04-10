//using JustABackup.Base;
//using JustABackup.Core.Entities;
//using JustABackup.Core.Services;
//using JustABackup.Database;
//using JustABackup.Database.Entities;
//using JustABackup.Database.Enum;
//using JustABackup.Database.Repositories;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Quartz;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace JustABackup.Core.ScheduledJobs
//{
//    public class DefaultScheduledJob : IJob
//    {
//        private class TransformBackupItem
//        {
//            public MappedBackupItem MappedBackupItem { get; set; }

//            public Func<Stream, Task> Execute { get; set; }

//            public int ID { get; set; }
//        }

//        private ILogger<DefaultScheduledJob> logger;

//        private IBackupJobRepository backupJobRepository;
//        private IProviderMappingService providerMappingService;

//        public DefaultScheduledJob(IBackupJobRepository backupJobRepository, IProviderMappingService providerMappingService, ILogger<DefaultScheduledJob> logger)
//        {
//            this.logger = logger;

//            this.backupJobRepository = backupJobRepository;
//            this.providerMappingService = providerMappingService;
//        }

//        public async Task Execute(IJobExecutionContext context)
//        {
//            int jobId = int.Parse(context.JobDetail.Key.Name);
//            int historyId = 0;

//            IDisposableList disposableList = new IDisposableList();

//            try
//            {
//                // load the job from the database and create a history point for this scheduled execution
//                BackupJob job = await backupJobRepository.Get(jobId);
//                DateTime? lastRun = await backupJobRepository.GetLastRun(jobId);
//                historyId = await backupJobRepository.AddHistory(job.ID);

//                // sort the providers so they are executed in the correct order
//                var providers = job.Providers.OrderBy(p => p.Order);

//                // load and create the backup and storage providers
//                IBackupProvider backupProvider = await providerMappingService.CreateProvider<IBackupProvider>(providers.FirstOrDefault());
//                IStorageProvider storageProvider = await providerMappingService.CreateProvider<IStorageProvider>(providers.LastOrDefault());
//                disposableList.AddRange(new IDisposable[] { backupProvider, storageProvider });

//                // load all the transform providers
//                List<ITransformProvider> transformProviders = new List<ITransformProvider>();
//                foreach (var tp in providers.Where(p => p.Provider.Type == ProviderType.Transform))
//                    transformProviders.Add(await providerMappingService.CreateProvider<ITransformProvider>(tp));
//                disposableList.AddRange(transformProviders);

//                // fetch all items from the backup providers
//                var items = await backupProvider.GetItems(lastRun);

//                List<List<TransformBackupItem>> transformExecuteList = new List<List<TransformBackupItem>>(transformProviders.Count());

//                Dictionary<ITransformProvider, IEnumerable<MappedBackupItem>> transformers = new Dictionary<ITransformProvider, IEnumerable<MappedBackupItem>>(transformProviders.Count());
//                for (int i = 0; i < transformProviders.Count(); i++)
//                {
//                    if (i > 0)
//                    {
//                        var mappedItems = await transformProviders[i].MapInput(transformers.Last().Value.Select(x => x.Output));
//                        transformers.Add(transformProviders[i], mappedItems);

//                        List<TransformBackupItem> subTransformExecuteList = new List<TransformBackupItem>();
//                        foreach (var mappedItem in mappedItems)
//                        {
//                            int currentMappedIndex = i;
//                            Func<Stream, Task> action = async (stream) =>
//                            {
//                                Dictionary<BackupItem, Stream> dictionary = new Dictionary<BackupItem, Stream>();
//                                foreach (var backupItem in mappedItem.Input)
//                                {
//                                    ByteBufferStream ms = disposableList.CreateAndAdd<ByteBufferStream>();
//                                    var transformBackupItem = transformExecuteList[currentMappedIndex - 1].FirstOrDefault(x => x.MappedBackupItem.Output == backupItem);
//                                    await transformBackupItem.Execute(ms);
//                                    dictionary.Add(backupItem, ms);
//                                }

//                                await transformProviders[currentMappedIndex].TransformItem(mappedItem.Output, stream, dictionary);
//                            };

//                            subTransformExecuteList.Add(new TransformBackupItem { MappedBackupItem = mappedItem, Execute = action });
//                        }
//                        transformExecuteList.Add(subTransformExecuteList);
//                    }
//                    else
//                    {
//                        var mappedItems = await transformProviders[i].MapInput(items);
//                        transformers.Add(transformProviders[i], mappedItems);

//                        List<TransformBackupItem> subTransformExecuteList = new List<TransformBackupItem>();
//                        foreach (var mappedItem in mappedItems)
//                        {
//                            int currentMappedIndex = i;
//                            Func<Stream, Task> action = async (stream) =>
//                            {
//                                Dictionary<BackupItem, Stream> dictionary = new Dictionary<BackupItem, Stream>();
//                                foreach (var backupItem in mappedItem.Input)
//                                {
//                                    Stream itemStream = await disposableList.CreateAndAdd(async () => await backupProvider.OpenRead(backupItem));
//                                    dictionary.Add(backupItem, itemStream);
//                                }

//                                await transformProviders[currentMappedIndex].TransformItem(mappedItem.Output, stream, dictionary);
//                            };

//                            subTransformExecuteList.Add(new TransformBackupItem { MappedBackupItem = mappedItem, Execute = action });
//                        }
//                        transformExecuteList.Add(subTransformExecuteList);
//                    }
//                }

//                if (transformProviders.Count() > 0)
//                {
//                    foreach (var mappedItem in transformExecuteList.Last())
//                    {
//                        using (ByteBufferStream outputStream = new ByteBufferStream())
//                        {
//                            await mappedItem.Execute(outputStream);

//                            bool storeItem = await storageProvider.StoreItem(mappedItem.MappedBackupItem.Output, outputStream);
//                        }
//                    }
//                }
//                else
//                {
//                    foreach (var item in items)
//                    {
//                        using (Stream itemStream = await backupProvider.OpenRead(item))
//                        {
//                            await storageProvider.StoreItem(item, itemStream);
//                        }
//                    }
//                }

//                await backupJobRepository.UpdateHistory(historyId, ExitCode.Success, "Backup completed successfully.");
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, $"Backup failed with ID {jobId} and History ID {historyId}.");

//                if (historyId > 0)
//                    await backupJobRepository.UpdateHistory(historyId, ExitCode.Failed, $"Backup failed with message: {ex.Message} ({ex.GetType()})");
//            }
//            finally
//            {
//                foreach (IDisposable item in disposableList)
//                {
//                    try
//                    {
//                        item?.Dispose();
//                    }
//                    catch (NotImplementedException)
//                    {
//                        // ignore this exception
//                    }
//                    catch (Exception ex)
//                    {
//                        // log every other error
//                        logger.LogError(ex, $"Failed to dispose item.");
//                    }
//                }

//                disposableList.Clear();
//                disposableList = null;
//            }
//        }
//    }
//}
