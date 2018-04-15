using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models.Authentication
{
    public class ConfigureAuthenticatedSessionModel : BaseViewModel
    {
        public int? ID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
        
        [Required]
        public int AuthenticationProvider { get; set; }

        public IEnumerable<Dictionary<string, string>> Providers { get; set; }

        public Dictionary<int, int> ProviderInstances { get; set; }

        public ConfigureAuthenticatedSessionModel()
        {
            ProviderInstances = new Dictionary<int, int>();
        }
    }
}
