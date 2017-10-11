using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.Core.Entities.Database
{
    internal class ProviderProperty
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string TypeName { get; set; }

        public string Description { get; set; }

        [Required]
        public int Type { get; set; }
    }
}
