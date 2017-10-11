using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.Core.Entities.Database
{
    internal class Provider
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public ProviderType Type { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Namespace { get; set; }

        [Required]
        public string Version { get; set; }

        [Required]
        public bool IsProcessed { get; set; }

        public List<ProviderProperty> Properties { get; set; }
    }
}
