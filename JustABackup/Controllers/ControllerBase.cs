using JustABackup.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Controllers
{
    public class ControllerBase : Controller
    {
        protected T CreateModel<T>() where T : BaseViewModel, new()
        {
            return new T();
        }

        protected T CreateModel<T>(string title) where T : BaseViewModel, new()
        {
            return new T()
            {
                Title = title
            };
        }

        protected T CreateModel<T>(string title, string description) where T : BaseViewModel, new()
        {
            return new T()
            {
                Title = title,
                TitleDescription = description
            };
        }
    }
}
