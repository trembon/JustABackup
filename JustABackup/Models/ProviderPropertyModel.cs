using JustABackup.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models
{
    public class ProviderPropertyModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public dynamic Value { get; set; }

        public string Template { get; set; }
    }
}
