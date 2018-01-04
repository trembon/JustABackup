using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models
{
    public class CreateAuthenicatedSession
    {
        public CreateAuthenticatedSessionModel Base { get; set; }

        public CreateProviderModel ProviderInstance { get; set; }

        public string SessionData { get; set; }
    }
}
