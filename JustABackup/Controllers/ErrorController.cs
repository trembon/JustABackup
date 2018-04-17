using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JustABackup.Models.Error;
using Microsoft.AspNetCore.Mvc;

namespace JustABackup.Controllers
{
    public class ErrorController : ControllerBase
    {
        [Route("error/{statusCode}")]
        public IActionResult Index(int statusCode)
        {
            StatusCodeModel model = CreateModel<StatusCodeModel>();
            model.StatusCode = statusCode;

            return View(model);
        }

        [Route("error/exception")]
        public IActionResult Exception()
        {
            StatusCodeModel model = CreateModel<StatusCodeModel>();
            model.StatusCode = 500;

            return View(model);
        }
    }
}