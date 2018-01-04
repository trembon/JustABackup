using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models
{
    public class AuthenticatedSessionModel
    {
        public long ID { get; set; }

        public string Name { get; set; }

        public string Provider { get; set; }

        public bool HasChangedModel { get; set; }
    }
}
