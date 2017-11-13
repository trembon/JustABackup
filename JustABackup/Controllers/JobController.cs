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
        public IActionResult Index()
        {
            ListJobsModel model = CreateModel<ListJobsModel>("Scheduled Backups");

            model.Jobs = dbContext
                .Jobs
                .OrderBy(j => j.Name)
                .Select(j => new JobModel
                {
                    ID = j.ID,
                    Name = j.Name,
                    BackupProvider = j.BackupProvider.Provider.Name,
                    StorageProvider = j.StorageProvider.Provider.Name
                })
                .ToList();

            model.Jobs.ForEach(async j => j.NextRun = await schedulerService.GetNextRunTime(j.ID));

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            CreateJobModel model = CreateModel<CreateJobModel>("Create Schedule");
            
            var backupProviders = dbContext.Providers.Where(p => p.Type == ProviderType.Backup).Select(p => new { ID = p.ID, Name = p.Name }).ToList();
            model.BackupProviders = new SelectList(backupProviders, "ID", "Name");

            var storageProviders = dbContext.Providers.Where(p => p.Type == ProviderType.Storage).Select(p => new { ID = p.ID, Name = p.Name }).ToList();
            model.StorageProviders = new SelectList(storageProviders, "ID", "Name");

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

            return RedirectToAction("CreateJobBackup");
        }

        [HttpGet]
        public IActionResult CreateJobBackup()
        {
            CreateJob createJob = HttpContext.Session.GetObject<CreateJob>("CreateJob");
            if (createJob == null)
                return RedirectToAction("Index", "Home");

            CreateJobProviderModel model = CreateModel<CreateJobProviderModel>("Create Schedule");
            model.Action = nameof(CreateJobBackup);
            
            var provider = dbContext.Providers.Include(x => x.Properties).FirstOrDefault(p => p.ID == createJob.Base.BackupProvider);

            model.ProviderName = provider.Name;
            model.Properties = provider.Properties.Select(x => new Models.ProviderPropertyModel { Name = x.Name, Description = x.Description, Template = typeMappingService.GetTemplateFromType(x.Type) }).ToList();

            model.Action = nameof(CreateJobBackup);
            return View("CreateJobProvider", model);
        }

        [HttpPost]
        public IActionResult CreateJobBackup(CreateJobProviderModel model)
        {
            CreateJob createJob = HttpContext.Session.GetObject<CreateJob>("CreateJob");
            if (createJob == null)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                createJob.BackupProvider = model;

                HttpContext.Session.SetObject("CreateJob", createJob);

                return RedirectToAction("CreateJobStorage");
            }

            return View("CreateJobProvider", model);
        }

        [HttpGet]
        public IActionResult CreateJobStorage()
        {
            CreateJob createJob = HttpContext.Session.GetObject<CreateJob>("CreateJob");
            if (createJob == null)
                return RedirectToAction("Index", "Home");

            CreateJobProviderModel model = CreateModel<CreateJobProviderModel>("Create Schedule");
            model.Action = nameof(CreateJobStorage);
            
            var provider = dbContext.Providers.Include(x => x.Properties).FirstOrDefault(p => p.ID == createJob.Base.StorageProvider);

            model.ProviderName = provider.Name;
            model.Properties = provider.Properties.Select(x => new Models.ProviderPropertyModel { Name = x.Name, Description = x.Description, Template = typeMappingService.GetTemplateFromType(x.Type) }).ToList();

            return View("CreateJobProvider", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateJobStorage(CreateJobProviderModel model)
        {
            CreateJob createJob = HttpContext.Session.GetObject<CreateJob>("CreateJob");
            if (createJob == null)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                createJob.StorageProvider = model;
                
                BackupJob job = new BackupJob();
                job.Name = createJob.Base.Name;

                Provider dbBackupProvider = dbContext.Providers.Include(p => p.Properties).FirstOrDefault(p => p.ID == createJob.Base.BackupProvider);
                Provider dbStorageProvider = dbContext.Providers.Include(p => p.Properties).FirstOrDefault(p => p.ID == createJob.Base.StorageProvider);

                ProviderInstance backupProvider = new ProviderInstance();
                backupProvider.Provider = dbBackupProvider;
                foreach (var property in createJob.BackupProvider.Properties)
                {
                    ProviderInstanceProperty instanceProperty = new ProviderInstanceProperty();
                    instanceProperty.Value = property.Value;
                    instanceProperty.Property = dbBackupProvider.Properties.FirstOrDefault(p => p.Name == property.Name);
                    backupProvider.Values.Add(instanceProperty);
                }
                job.BackupProvider = backupProvider;

                ProviderInstance storageProvider = new ProviderInstance();
                storageProvider.Provider = dbStorageProvider;
                foreach (var property in createJob.StorageProvider.Properties)
                {
                    ProviderInstanceProperty instanceProperty = new ProviderInstanceProperty();
                    instanceProperty.Value = property.Value;
                    instanceProperty.Property = dbStorageProvider.Properties.FirstOrDefault(p => p.Name == property.Name);
                    storageProvider.Values.Add(instanceProperty);
                }
                job.StorageProvider = storageProvider;

                dbContext.Jobs.Add(job);
                await dbContext.SaveChangesAsync();

                await schedulerService.CreateScheduledJob(job.ID, createJob.Base.CronSchedule);

                HttpContext.Session.Remove("CreateJob");
                return RedirectToAction("Index", "Home");
            }

            model.Action = nameof(CreateJobStorage);
            return View("CreateJobProvider", model);
        }

        public async Task<IActionResult> Start(int id)
        {
            await schedulerService.TriggerJob(id);
            return RedirectToAction("Index", "Home");
        }
    }
}