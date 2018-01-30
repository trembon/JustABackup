using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JustABackup.Models;
using Microsoft.EntityFrameworkCore;
using JustABackup.Database;
using JustABackup.Database.Repositories;
using JustABackup.Models.Home;

namespace JustABackup.Controllers
{
    public class HomeController : ControllerBase
    {
        private IBackupJobRepository backupJobRepository;

        public HomeController(IBackupJobRepository backupJobRepository)
        {
            this.backupJobRepository = backupJobRepository;
        }

        public async Task<IActionResult> Index()
        {
            var model = CreateModel<ListJobHistoryModel>("Backup History");

            var history = await backupJobRepository.GetHistory(15);

            model.JobHistory = history
                .Select(jh => new JobHistoryModel
                {
                    JobID = jh.Job.ID,
                    JobName = jh.Job.Name,
                    Started = jh.Started,
                    RunTime = jh.Completed - jh.Started,
                    Status = jh.Status.ToString(),
                    Message = jh.Message
                })
                .ToList();

            return View(model);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
