using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JustABackup.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JustABackup.Controllers
{
    [Produces("application/json")]
    [Route("api/job")]
    public class JobAPIController : Controller
    {
        private ISchedulerService schedulerService;

        public JobAPIController(ISchedulerService schedulerService)
        {
            this.schedulerService = schedulerService;
        }

        [Route("start")]
        public async Task<IActionResult> Start(int[] ids)
        {
            Dictionary<int, bool> result = new Dictionary<int, bool>();

            foreach (int id in ids)
            {
                try
                {
                    await schedulerService.TriggerJob(id);
                    result.Add(id, true);
                }
                catch (Exception)
                {
                    // TOOD: log
                    result.Add(id, false);
                }
            }

            return Ok(result);
        }

        [Route("resume")]
        public async Task<IActionResult> Resume(int[] ids)
        {
            Dictionary<int, DateTime?> result = new Dictionary<int, DateTime?>(ids.Length);

            foreach (int id in ids)
            {
                try
                {
                    await schedulerService.ResumeJob(id);
                    result.Add(id, await schedulerService.GetNextRunTime(id));
                }
                catch (Exception)
                {
                    // TOOD: log
                    result.Add(id, null);
                }
            }

            return Ok(result);
        }

        [Route("pause")]
        public async Task<IActionResult> Pause(int[] ids)
        {
            Dictionary<int, DateTime?> result = new Dictionary<int, DateTime?>(ids.Length);

            foreach (int id in ids)
            {
                try
                {
                    await schedulerService.PauseJob(id);
                    result.Add(id, await schedulerService.GetNextRunTime(id));
                }
                catch (Exception)
                {
                    // TOOD: log
                    result.Add(id, null);
                }
            }

            return Ok(result);
        }
    }
}