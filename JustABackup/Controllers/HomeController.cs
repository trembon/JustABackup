using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JustABackup.DAL.Contexts;
using JustABackup.Models;
using Microsoft.EntityFrameworkCore;

namespace JustABackup.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new ListJobsModel();

            using (var context = new DefaultContext())
            {
                model.Jobs = context.Jobs.Include(x => x.History).ToList();
            }

            return View(model);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
