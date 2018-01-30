using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models.Authentication
{
    public class ListAuthenticatedSessionModel : BaseViewModel
    {
        public List<AuthenticatedSessionModel> Sessions { get; set; }
    }
}
