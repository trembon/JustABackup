using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.Database.Entities
{
    public class AuthenticatedSession
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public ProviderInstance Provider { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public bool HasChangedModel { get; set; }

        public byte[] SessionData { get; set; }
        
        public List<ProviderInstanceProperty> Values { get; set; }

        public AuthenticatedSession()
        {
            HasChangedModel = false;
            Values = new List<ProviderInstanceProperty>();
        }
    }
}
