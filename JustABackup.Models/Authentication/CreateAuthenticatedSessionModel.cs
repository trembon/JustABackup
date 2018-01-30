using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models.Authentication
{
    public class CreateAuthenticatedSessionModel : BaseViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
        
        [Required]
        public int AuthenticationProvider { get; set; }

        public SelectList AuthenticationProviders { get; set; }
    }
}
