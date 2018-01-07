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

namespace JustABackup.Controllers
{
    public class JobController : ControllerBase
    {
        private DefaultContext dbContext;
        private ISchedulerService schedulerService;
        private IProviderMappingService typeMappingService;

        private const string JOB_STORAGE_KEY = "ConfigureJob";

        public JobController(DefaultContext dbContext, ISchedulerService schedulerService, IProviderMappingService typeMappingService)
        {
            this.dbContext = dbContext;
            this.schedulerService = schedulerService;
            this.typeMappingService = typeMappingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ListJobsModel model = CreateModel<ListJobsModel>("Scheduled Backups");

            model.Jobs = await dbContext
                .Jobs
                .OrderBy(j => j.Name)
                .Select(j => new JobModel
                {
                    ID = j.ID,
                    Name = j.Name,
                    LastRun = j.History.OrderByDescending(h => h.Started).Select(h => h.Started).FirstOrDefault(),
                    HasChangedModel = j.HasChangedModel
                })
                .ToListAsync();

            model.Jobs.ForEach(async j => j.NextRun = await schedulerService.GetNextRunTime(j.ID));

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            BackupJob job = await dbContext
                .Jobs
                .Include(j => j.Providers)
                .ThenInclude(x => x.Provider)
                .Include(j => j.Providers)
                .ThenInclude(x => x.Values)
                .ThenInclude(x => x.Property)
                .FirstOrDefaultAsync(j => j.ID == id);

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
            CreateJobModel model;

            if (id.HasValue && id > 0)
            {
                BackupJob job = await dbContext
                    .Jobs
                    .Include(j => j.Providers)
                    .ThenInclude(x => x.Provider)
                    .Include(j => j.Providers)
                    .ThenInclude(x => x.Values)
                    .ThenInclude(x => x.Property)
                    .FirstOrDefaultAsync(j => j.ID == id);

                if (job == null)
                    return NotFound();

                var providers = job.Providers.OrderBy(p => p.Order);

                model = CreateModel<ModifyJobModel>("Modify Schedule");
                model.ID = job.ID;
                model.Name = job.Name;
                model.CronSchedule = await schedulerService.GetCronSchedule(id.Value);
                model.BackupProvider = providers.FirstOrDefault().Provider.ID;
                model.StorageProvider = providers.LastOrDefault().Provider.ID;
                model.TransformProvider = providers.Where(p => p.Provider.Type == ProviderType.Transform).Select(tp => tp.Provider.ID).ToArray();
            }
            else
            {
                model = CreateModel<CreateJobModel>("Create Schedule");
            }
            
            var backupProviders = dbContext.Providers.Where(p => p.Type == ProviderType.Backup).Select(p => new { ID = p.ID, Name = p.Name }).ToList();
            model.BackupProviders = new SelectList(backupProviders, "ID", "Name");

            var storageProviders = dbContext.Providers.Where(p => p.Type == ProviderType.Storage).Select(p => new { ID = p.ID, Name = p.Name }).ToList();
            model.StorageProviders = new SelectList(storageProviders, "ID", "Name");

            var transformProviders = dbContext.Providers.Where(p => p.Type == ProviderType.Transform).Select(p => new { ID = p.ID, Name = p.Name }).ToList();
            model.TransformProviders = new SelectList(transformProviders, "ID", "Name");
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(CreateJobModel model)
        {
            if (ModelState.IsValid)
            {
                CreateJob createJob = new CreateJob { Base = model };
                HttpContext.Session.SetObject(JOB_STORAGE_KEY, createJob);

                if (model.ID > 0)
                {
                    var firstProvider = await dbContext.ProviderInstances.Where(pi => pi.Job.ID == model.ID).OrderBy(pi => pi.Order).FirstOrDefaultAsync();
                    if(firstProvider != null)
                        return RedirectToAction("ConfigureJobProvider", new { id = firstProvider.ID });
                }

                return RedirectToAction("ConfigureJobProvider");
            }
            
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfigureJobProvider(int index = 0, int id = 0)
        {
            CreateJob createJob = HttpContext.Session.GetObject<CreateJob>(JOB_STORAGE_KEY);
            if (createJob == null)
                return RedirectToAction("Index", "Home");

            CreateJobProviderModel model = null;

            Provider provider = dbContext.Providers.Include(x => x.Properties).FirstOrDefault(p => p.ID == createJob.ProviderIDs[index]);
            Dictionary<int, string> values = new Dictionary<int, string>();
            
            if (id > 0)
            {
                model = CreateModel<ModifyJobProviderModel>("Modify Schedule");
                model.ID = id;

                values = await dbContext.ProviderInstances.Where(pi => pi.ID == id).SelectMany(pi => pi.Values).Include(v => v.Property).ToDictionaryAsync(k => k.Property.ID, v => v.Value);
            }
            else
            {
                model = CreateModel<CreateJobProviderModel>("Create Schedule");
            }
            
            model.CurrentIndex = index;
            model.ProviderName = provider.Name;
            model.Properties = provider.Properties.Select(x => new Models.ProviderPropertyModel // TODO: place in a common place?
            {
                Name = x.Name,
                Description = x.Description,
                Template = typeMappingService.GetTemplateFromType(x.Type),
                Value = values.ContainsKey(x.ID) ? typeMappingService.GetObjectFromString(values[x.ID], x.Type) : null,
                ViewData = x.GenericType != null ? new { Type = x.GenericType } : null
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfigureJobProvider(CreateJobProviderModel model)
        {
            CreateJob createJob = HttpContext.Session.GetObject<CreateJob>(JOB_STORAGE_KEY);
            if (createJob == null)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                createJob.Providers.Add(model);

                HttpContext.Session.SetObject(JOB_STORAGE_KEY, createJob);

                int nextIndex = model.CurrentIndex + 1;
                if (nextIndex >= createJob.ProviderIDs.Length)
                {
                    await AddJobToDatabase(createJob);
                    HttpContext.Session.Remove(JOB_STORAGE_KEY);

                    return RedirectToAction("Index");
                }
                else
                {
                    int? nextId = null;
                    if(createJob.Base.ID > 0)
                    {
                        var instances = await dbContext.ProviderInstances.Where(pi => pi.Job.ID == createJob.Base.ID).OrderBy(pi => pi.Order).ToListAsync();
                        var nextJob = await dbContext.ProviderInstances.Where(pi => pi.Job.ID == createJob.Base.ID).OrderBy(pi => pi.Order).Skip(nextIndex).FirstOrDefaultAsync();
                        nextId = nextJob?.ID;
                    }

                    return RedirectToAction("ConfigureJobProvider", new { index = nextIndex, id = nextId });
                }
            }

            return View(model);
        }
        #endregion
        
        private async Task AddJobToDatabase(CreateJob createJob)
        {
            BackupJob job = null;
            if (createJob.Base.ID > 0)
            {
                job = await dbContext.Jobs.Include(j => j.Providers).FirstOrDefaultAsync(j => j.ID == createJob.Base.ID);
                job.HasChangedModel = false;
                job.Providers.Clear();
            }
            else
            {
                job = new BackupJob();
                dbContext.Jobs.Add(job);
            }

            job.Name = createJob.Base.Name;
            
            for(int i = 0; i < createJob.Providers.Count; i++)
            {
                Provider provider = dbContext.Providers.Include(p => p.Properties).FirstOrDefault(p => p.ID == createJob.ProviderIDs[i]);
                ProviderInstance providerInstance = createJob.Providers[i].CreateProviderInstance(provider);
                providerInstance.Order = i;
                job.Providers.Add(providerInstance);
            }

            await dbContext.SaveChangesAsync();

            if (createJob.Base.ID > 0)
            {
                // TODO: UpdateScheduledJob
            }
            else
            {
                await schedulerService.CreateScheduledJob(job.ID, createJob.Base.CronSchedule);
            }
        }

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