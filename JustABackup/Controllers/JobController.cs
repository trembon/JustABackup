using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JustABackup.Core.Extenssions;
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
        private ITypeMappingService typeMappingService;

        public JobController(DefaultContext dbContext, ISchedulerService schedulerService, ITypeMappingService typeMappingService)
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
                .Include(j => j.BackupProvider)
                .Include(j => j.BackupProvider.Provider)
                .Include(j => j.BackupProvider.Values)
                .ThenInclude(x => x.Property)
                .Include(j => j.StorageProvider)
                .Include(j => j.StorageProvider.Provider)
                .Include(j => j.StorageProvider.Values)
                .ThenInclude(x => x.Property)
                .Include(j => j.TransformProviders)
                .ThenInclude(x => x.Provider)
                .Include(j => j.TransformProviders)
                .ThenInclude(x => x.Values)
                .ThenInclude(x => x.Property)
                .FirstOrDefaultAsync(j => j.ID == id);

            if (job == null)
                return NotFound();

            JobDetailModel model = CreateModel<JobDetailModel>("Scheduled Backup");

            model.ID = id;
            model.Name = job.Name;
            model.HasChangedModel = job.HasChangedModel;
            model.BackupProvider = job.BackupProvider.Provider.Name;
            model.StorageProvider = job.StorageProvider.Provider.Name;
            model.TransformProviders = job.TransformProviders.Select(tp => tp.Provider.Name);

            model.CronSchedule = await schedulerService.GetCronSchedule(id);

            return View(model);
        }

        #region Create scheduled job
        [HttpGet]
        public IActionResult Create()
        {
            CreateJobModel model = CreateModel<CreateJobModel>("Create Schedule");
            
            var backupProviders = dbContext.Providers.Where(p => p.Type == ProviderType.Backup).Select(p => new { ID = p.ID, Name = p.Name }).ToList();
            model.BackupProviders = new SelectList(backupProviders, "ID", "Name");

            var storageProviders = dbContext.Providers.Where(p => p.Type == ProviderType.Storage).Select(p => new { ID = p.ID, Name = p.Name }).ToList();
            model.StorageProviders = new SelectList(storageProviders, "ID", "Name");

            var transformProviders = dbContext.Providers.Where(p => p.Type == ProviderType.Transform).Select(p => new { ID = p.ID, Name = p.Name }).ToList();
            model.TransformProviders = new SelectList(transformProviders, "ID", "Name");

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(CreateJobModel model)
        {
            if (ModelState.IsValid)
            {
                CreateJob createJob = new CreateJob { Base = model };
                HttpContext.Session.SetObject("CreateJob", createJob);
            }

            return RedirectToAction("ConfigureJobProvider");
        }

        [HttpGet]
        public IActionResult ConfigureJobProvider(int index = 0)
        {
            CreateJob createJob = HttpContext.Session.GetObject<CreateJob>("CreateJob");
            if (createJob == null)
                return RedirectToAction("Index", "Home");
            
            var provider = dbContext.Providers.Include(x => x.Properties).FirstOrDefault(p => p.ID == createJob.ProviderIDs[index]);

            ConfigureJobProviderModel model = CreateModel<ConfigureJobProviderModel>("Create Schedule");
            model.CurrentIndex = index;
            model.ProviderName = provider.Name;
            model.Properties = provider.Properties.Select(x => new Models.ProviderPropertyModel { Name = x.Name, Description = x.Description, Template = typeMappingService.GetTemplateFromType(x.Type) }).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfigureJobProvider(ConfigureJobProviderModel model)
        {
            CreateJob createJob = HttpContext.Session.GetObject<CreateJob>("CreateJob");
            if (createJob == null)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                createJob.Providers.Add(model);

                HttpContext.Session.SetObject("CreateJob", createJob);

                int nextIndex = model.CurrentIndex + 1;
                if (nextIndex >= createJob.ProviderIDs.Length)
                {
                    await AddJobToDatabase(createJob);
                    HttpContext.Session.Remove("CreateJob");

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("ConfigureJobProvider", new { index = nextIndex });
                }
            }

            return View(model);
        }
        #endregion

        #region Modify scheduled job
        [HttpGet]
        public async Task<IActionResult> Modify(int id)
        {
            BackupJob job = await dbContext
                .Jobs
                .Include(j => j.BackupProvider)
                .Include(j => j.BackupProvider.Provider)
                .Include(j => j.BackupProvider.Values)
                .ThenInclude(x => x.Property)
                .Include(j => j.StorageProvider)
                .Include(j => j.StorageProvider.Provider)
                .Include(j => j.StorageProvider.Values)
                .ThenInclude(x => x.Property)
                .Include(j => j.TransformProviders)
                .ThenInclude(x => x.Provider)
                .Include(j => j.TransformProviders)
                .ThenInclude(x => x.Values)
                .ThenInclude(x => x.Property)
                .FirstOrDefaultAsync(j => j.ID == id);
            
            if (job == null)
                return NotFound();

            ModifyJobModel model = CreateModel<ModifyJobModel>("Modify Schedule");
            model.ID = job.ID;
            model.Name = job.Name;
            model.CronSchedule = await schedulerService.GetCronSchedule(id);
            model.BackupProvider = job.BackupProvider.Provider.ID;
            model.StorageProvider = job.StorageProvider.Provider.ID;
            model.TransformProvider = job.TransformProviders.Select(tp => tp.Provider.ID).ToArray();

            var backupProviders = dbContext.Providers.Where(p => p.Type == ProviderType.Backup).Select(p => new { ID = p.ID, Name = p.Name }).ToList();
            model.BackupProviders = new SelectList(backupProviders, "ID", "Name");

            var storageProviders = dbContext.Providers.Where(p => p.Type == ProviderType.Storage).Select(p => new { ID = p.ID, Name = p.Name }).ToList();
            model.StorageProviders = new SelectList(storageProviders, "ID", "Name");

            var transformProviders = dbContext.Providers.Where(p => p.Type == ProviderType.Transform).Select(p => new { ID = p.ID, Name = p.Name }).ToList();
            model.TransformProviders = new SelectList(transformProviders, "ID", "Name");

            return View(model);
        }

        [HttpPost]
        public IActionResult Modify()
        {

            return View();
        }
        #endregion

        private async Task AddJobToDatabase(CreateJob createJob)
        {
            BackupJob job = new BackupJob();
            job.Name = createJob.Base.Name;
            
            for(int i = 0; i < createJob.Providers.Count; i++)
            {
                Provider provider = dbContext.Providers.Include(p => p.Properties).FirstOrDefault(p => p.ID == createJob.ProviderIDs[i]);
                ProviderInstance providerInstance = createJob.Providers[i].CreateProviderInstance(provider);

                if (i == 0)
                {
                    job.BackupProvider = providerInstance;
                }
                else if (i >= createJob.Providers.Count - 1)
                {
                    job.StorageProvider = providerInstance;
                }
                else
                {
                    job.TransformProviders.Add(providerInstance);
                }
            }

            dbContext.Jobs.Add(job);
            await dbContext.SaveChangesAsync();

            await schedulerService.CreateScheduledJob(job.ID, createJob.Base.CronSchedule);
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