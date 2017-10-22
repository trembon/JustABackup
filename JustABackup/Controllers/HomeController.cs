using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JustABackup.Models;
using Microsoft.EntityFrameworkCore;
using JustABackup.Database;

namespace JustABackup.Controllers
{
    public class HomeController : Controller
    {
        private DefaultContext context;

        public HomeController(DefaultContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            var model = new ListJobHistoryModel();

            model.JobHistory = context
                .JobHistory
                .OrderByDescending(jh => jh.Started)
                .Take(15)
                .Select(jh => new JobHistoryModel
                {
                    JobID = jh.Job.ID,
                    JobName = jh.Job.Name,
                    Started = jh.Started,
                    RunTime = jh.Completed - jh.Started,
                    Status = jh.Status,
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
