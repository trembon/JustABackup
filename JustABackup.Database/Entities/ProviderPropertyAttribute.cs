using JustABackup.Database.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.Database.Entities
{
    public class ProviderPropertyAttribute
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public PropertyAttribute Name { get; set; }

        public string Value { get; set; }

        public ProviderPropertyAttribute()
        {
        }

        public ProviderPropertyAttribute(PropertyAttribute name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
