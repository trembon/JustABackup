using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JustABackup.Core.Extensions;
using JustABackup.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using JustABackup.Base;
using System.Reflection;
using JustABackup.Database;
using JustABackup.Database.Enum;
using JustABackup.Database.Entities;
using Quartz;
using Quartz.Impl;
using JustABackup.Core.Services;
using JustABackup.Database.Repositories;
using JustABackup.Models.Job;
using Microsoft.Extensions.Logging;

namespace JustABackup.Controllers
{
    public class JobController : ControllerBase
    {
        private ILogger<JobController> logger;

        private ISchedulerService schedulerService;
        private IProviderRepository providerRepository;
        private IBackupJobRepository backupJobRepository;
        private IProviderMappingService providerMappingService;

        public JobController(IBackupJobRepository backupJobRepository, IProviderRepository providerRepository, ISchedulerService schedulerService, IProviderMappingService providerMappingService, ILogger<JobController> logger)
        {
            this.logger = logger;

            this.schedulerService = schedulerService;
            this.providerRepository = providerRepository;
            this.backupJobRepository = backupJobRepository;
            this.providerMappingService = providerMappingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ListJobsModel model = CreateModel<ListJobsModel>();

            var jobs = await backupJobRepository.Get();

            model.Jobs = jobs
                .Select(j => new JobModel
                {
                    ID = j.ID,
                    Name = j.Name,
                    HasChangedModel = j.HasChangedModel
                })
                .ToList();

            model.Jobs.ForEach(async j =>
            {
                j.NextRun = await schedulerService.GetNextRunTime(j.ID);
                j.LastRun = await backupJobRepository.GetLastRun(j.ID);
            });

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            BackupJob job = await backupJobRepository.Get(id);
            if (job == null)
                return NotFound();
            
            var providers = job.Providers.OrderBy(p => p.Order);

            JobDetailModel model = CreateModel<JobDetailModel>("Scheduled Backup");
            model.ID = id;
            model.Name = job.Name;
            model.HasChangedModel = job.HasChangedModel;
            model.BackupProvider = providers.FirstOrDefault().Provider.Name;
            model.StorageProvider = providers.LastOrDefault().Provider.Name;
            model.TransformProviders = providers.Where(p => p.Provider.Type == ProviderType.Transform).Select(tp => tp.Provider.Name);

            model.CronSchedule = await schedulerService.GetCronSchedule(id);

            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> Configure(int? id = null)
        {
            ConfigureJobModel model = CreateModel<ConfigureJobModel>();

            if (id.HasValue && id > 0)
            {
                BackupJob job = await backupJobRepository.Get(id.Value);
                if (job == null)
                    return NotFound();

                var providers = job.Providers.OrderBy(p => p.Order);
                
                model.ID = job.ID;
                model.Name = job.Name;
                model.CronSchedule = await schedulerService.GetCronSchedule(id.Value);
                model.BackupProvider = providers.FirstOrDefault().Provider.ID;
                model.StorageProvider = providers.LastOrDefault().Provider.ID;
                model.TransformProviders = providers.Where(p => p.Provider.Type == ProviderType.Transform).Select(tp => tp.Provider.ID).ToArray();

                model.ProviderInstances = job.Providers.ToDictionary(k => k.Provider.ID, v => v.ID);
            }
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigureJobModel model)
        {
            // TODO: find out why ModelState is invalid with unvalidated state (probably because of the IEnumerable/Dictionary Providers property)
            if (ModelState.ErrorCount == 0)
            {
                try
                {
                    int[] providerIDs = model.GetProviderIDs();

                    List<ProviderInstance> providerInstances = new List<ProviderInstance>(providerIDs.Length);
                    for (int i = 0; i < providerIDs.Length; i++)
                    {
                        Provider provider = await providerRepository.Get(providerIDs[i]);
                        ProviderInstance providerInstance = await providerMappingService.CreateProviderInstance(provider, model.Providers.ElementAt(i));
                        providerInstance.Order = i;
                        providerInstances.Add(providerInstance);
                    }

                    int jobId = await backupJobRepository.AddOrUpdate(model.ID, model.Name, providerInstances);
                    await schedulerService.CreateOrUpdate(jobId, model.CronSchedule);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to save job '{model.Name}' to database");
                }
            }

            return RedirectToAction("Index");
        }
    }
}