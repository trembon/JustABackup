using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.Database.Entities
{
    public class ProviderInstanceProperty
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public ProviderProperty Property { get; set; }
        
        public byte[] Value { get; set; }
    }
}
