using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models
{
    public abstract class BaseViewModel
    {
        public string Title { get; set; }

        public string TitleDescription { get; set; }
    }
}
