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
        private const string JOB_STORAGE_KEY = "ConfigureJob";

        private ILogger<JobController> logger;

        private ISchedulerService schedulerService;
        private IProviderRepository providerRepository;
        private IBackupJobRepository backupJobRepository;
        private IProviderMappingService providerMappingService;

        public JobController(IBackupJobRepository backupJobRepository, IProviderRepository providerRepository, ISchedulerService schedulerService, IProviderMappingService typeMappingService, ILogger<JobController> logger)
        {
            this.logger = logger;

            this.schedulerService = schedulerService;
            this.providerRepository = providerRepository;
            this.backupJobRepository = backupJobRepository;
            this.providerMappingService = typeMappingService;
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

        #region Create scheduled job
        [HttpGet]
        public async Task<IActionResult> Configure(int? id = null)
        {
            ConfigureJobModel model;

            if (id.HasValue && id > 0)
            {
                BackupJob job = await backupJobRepository.Get(id.Value);
                if (job == null)
                    return NotFound();

                var providers = job.Providers.OrderBy(p => p.Order);

                model = CreateModel<ConfigureJobModel>("Modify Schedule");
                model.ID = job.ID;
                model.Name = job.Name;
                model.CronSchedule = await schedulerService.GetCronSchedule(id.Value);
                model.BackupProvider = providers.FirstOrDefault().Provider.ID;
                model.StorageProvider = providers.LastOrDefault().Provider.ID;
                model.TransformProviders = providers.Where(p => p.Provider.Type == ProviderType.Transform).Select(tp => tp.Provider.ID).ToArray();
            }
            else
            {
                model = CreateModel<ConfigureJobModel>("Create Schedule");
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
            return View(model);
        }

        //[HttpPost]
        //public async Task<IActionResult> Configure(CreateJobModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        CreateJob createJob = new CreateJob { Base = model };
        //        HttpContext.Session.SetObject(JOB_STORAGE_KEY, createJob);

        //        int? firstProvider = await providerRepository.GetInstanceID(model.ID, 0);
        //        return RedirectToAction("ConfigureJobProvider", new { id = firstProvider });
        //    }

        //    return View(model);
        //}

        //[HttpGet]
        //public async Task<IActionResult> ConfigureJobProvider(int index = 0, int id = 0)
        //{
        //    CreateJob createJob = HttpContext.Session.GetObject<CreateJob>(JOB_STORAGE_KEY);
        //    if (createJob == null)
        //        return RedirectToAction("Index", "Home");

        //    CreateJobProviderModel model = null;

        //    Provider provider = await providerRepository.Get(createJob.ProviderIDs[index]);
        //    Dictionary<int, object> values = new Dictionary<int, object>();

        //    if (id > 0)
        //    {
        //        model = CreateModel<ModifyJobProviderModel>("Modify Schedule");
        //        model.ID = id;

        //        IEnumerable<ProviderInstanceProperty> propertyValues = await providerRepository.GetProviderValues(id);
        //        foreach(ProviderInstanceProperty propertyValue in propertyValues)
        //        {
        //            object parsedValue = await providerMappingService.GetPresentationValue(propertyValue);
        //            values.Add(propertyValue.Property.ID, parsedValue);
        //        }
        //    }
        //    else
        //    {
        //        model = CreateModel<CreateJobProviderModel>("Create Schedule");
        //    }

        //    model.CurrentIndex = index;
        //    model.ProviderName = provider.Name;
        //    model.Properties = provider.Properties.Select(x => new ProviderPropertyModel(x.Name, x.Description, providerMappingService.GetTemplateFromType(x.Type), values.ContainsKey(x.ID) ? values[x.ID] : null, x.Attributes?.ToDictionary(k => k.Name.ToString(), v => v.Value))).ToList();

        //    return View(model);
        //}

        //[HttpPost]
        //public async Task<IActionResult> ConfigureJobProvider(CreateJobProviderModel model)
        //{
        //    CreateJob createJob = HttpContext.Session.GetObject<CreateJob>(JOB_STORAGE_KEY);
        //    if (createJob == null)
        //        return RedirectToAction("Index", "Home");

        //    if (ModelState.IsValid)
        //    {
        //        createJob.Providers.Add(model);
        //        HttpContext.Session.SetObject(JOB_STORAGE_KEY, createJob);

        //        int nextIndex = model.CurrentIndex + 1;
        //        if (nextIndex >= createJob.ProviderIDs.Length)
        //        {
        //            await SaveJob(createJob);
        //            HttpContext.Session.Clear();

        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //            int? nextId = await providerRepository.GetInstanceID(createJob.Base.ID, nextIndex);
        //            return RedirectToAction("ConfigureJobProvider", new { index = nextIndex, id = nextId });
        //        }
        //    }

        //    return View(model);
        //}
        #endregion
        
        //private async Task SaveJob(CreateJob createJob)
        //{
        //    try
        //    {
        //        List<ProviderInstance> providerInstances = new List<ProviderInstance>(createJob.Providers.Count);
        //        for (int i = 0; i < createJob.Providers.Count; i++)
        //        {
        //            Provider provider = await providerRepository.Get(createJob.ProviderIDs[i]);
        //            ProviderInstance providerInstance = await providerMappingService.CreateProviderInstance(provider, createJob.Providers[i]);
        //            providerInstance.Order = i;
        //            providerInstances.Add(providerInstance);
        //        }

        //        int jobId = await backupJobRepository.AddOrUpdate(createJob.Base.ID, createJob.Base.Name, providerInstances);
        //        await schedulerService.CreateOrUpdate(jobId, createJob.Base.CronSchedule);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, $"Failed to save job '{createJob.Base.Name}' to database");
        //    }
        //}

        public async Task<IActionResult> Start(int[] ids)
        {
            foreach (int id in ids)
                await schedulerService.TriggerJob(id);

            return Ok(true);
        }

        public async Task<IActionResult> Resume(int[] ids)
        {
            Dictionary<int, DateTime?> result = new Dictionary<int, DateTime?>(ids.Length);

            foreach (int id in ids)
            {
                await schedulerService.ResumeJob(id);
                result.Add(id, await schedulerService.GetNextRunTime(id));
            }

            return Ok(result);
        }

        public async Task<IActionResult> Pause(int[] ids)
        {
            Dictionary<int, DateTime?> result = new Dictionary<int, DateTime?>(ids.Length);

            foreach (int id in ids)
            {
                await schedulerService.PauseJob(id);
                result.Add(id, await schedulerService.GetNextRunTime(id));
            }

            return Ok(result);
        }
    }
}