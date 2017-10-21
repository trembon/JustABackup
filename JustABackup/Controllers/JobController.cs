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

namespace JustABackup.Controllers
{
    public class JobController : Controller
    {
        private DefaultContext dbContext;

        public JobController(DefaultContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Create()
        {
            CreateJobModel model = new CreateJobModel();
            
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

        private string TypeToTemplate(int type)
        {
            switch (type)
            {
                case 0: return "String";
                case 1: return "Number";
                case 2: return "Boolean";

                default: return "String";
            }
        }

        [HttpGet]
        public IActionResult CreateJobBackup()
        {
            CreateJob createJob = HttpContext.Session.GetObject<CreateJob>("CreateJob");
            if (createJob == null)
                return RedirectToAction("Index", "Home");

            CreateJobProviderModel model = new CreateJobProviderModel();
            model.Action = nameof(CreateJobBackup);
            
            var provider = dbContext.Providers.Include(x => x.Properties).FirstOrDefault(p => p.ID == createJob.Base.BackupProvider);

            model.ProviderName = provider.Name;
            model.Properties = provider.Properties.Select(x => new Models.ProviderProperty { Name = x.Name, Description = x.Description, Template = TypeToTemplate(x.Type) }).ToList();

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

            CreateJobProviderModel model = new CreateJobProviderModel();
            model.Action = nameof(CreateJobStorage);
            
            var provider = dbContext.Providers.Include(x => x.Properties).FirstOrDefault(p => p.ID == createJob.Base.StorageProvider);

            model.ProviderName = provider.Name;
            model.Properties = provider.Properties.Select(x => new Models.ProviderProperty { Name = x.Name, Description = x.Description, Template = TypeToTemplate(x.Type) }).ToList();

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

                HttpContext.Session.Remove("CreateJob");
                return RedirectToAction("Index", "Home");
            }

            model.Action = nameof(CreateJobStorage);
            return View("CreateJobProvider", model);
        }

        public async Task<IActionResult> Start(int id)
        {
            //using (var context = new DefaultContext())
            //{
            //    BackupJob job = context.Jobs
            //        .Include(j => j.BackupProvider)
            //        .Include(j => j.BackupProvider.Provider)
            //        .Include(j => j.BackupProvider.Values)
            //        .ThenInclude(x => x.Property)
            //        .Include(j => j.StorageProvider)
            //        .Include(j => j.StorageProvider.Provider)
            //        .Include(j => j.StorageProvider.Values)
            //        .ThenInclude(x => x.Property)
            //        .FirstOrDefault(j => j.ID == id);

            //    BackupJobHistory history = new BackupJobHistory();
            //    history.Started = DateTime.Now;

            //    Type backupProviderType = Type.GetType(job.BackupProvider.Provider.Namespace);
            //    IBackupProvider backupProvider = Activator.CreateInstance(backupProviderType) as IBackupProvider;
            //    foreach(var property in job.BackupProvider.Values)
            //    {
            //        PropertyInfo propertyInfo = backupProviderType.GetProperty(property.Property.TypeName);
            //        object originalValueType = Convert.ChangeType(property.Value, propertyInfo.PropertyType);

            //        propertyInfo.SetValue(backupProvider, originalValueType);
            //    }

            //    Type storageProviderType = Type.GetType(job.StorageProvider.Provider.Namespace);
            //    IStorageProvider storageProvider = Activator.CreateInstance(storageProviderType) as IStorageProvider;
            //    foreach (var property in job.StorageProvider.Values)
            //    {
            //        PropertyInfo propertyInfo = storageProviderType.GetProperty(property.Property.TypeName);
            //        object originalValueType = Convert.ChangeType(property.Value, propertyInfo.PropertyType);

            //        propertyInfo.SetValue(storageProvider, originalValueType);
            //    }

            //    var items = await backupProvider.GetItems();
            //    foreach(var item in items)
            //    {
            //        using(var stream = await backupProvider.OpenRead(item))
            //        {
            //            await storageProvider.StoreItem(item, stream);
            //        }
            //    }

            //    history.Completed = DateTime.Now;
            //    history.Message = $"{items.Count()} files were copied.";
            //    history.Status = ExitCode.Success;
            //    job.History.Add(history);
            //    await context.SaveChangesAsync();
            //}

            return View();
        }
    }
}