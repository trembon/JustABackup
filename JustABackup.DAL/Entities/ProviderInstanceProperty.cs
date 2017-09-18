﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.DAL.Entities
{
    public class ProviderInstanceProperty
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public ProviderProperty Property { get; set; }
        
        public string Value { get; set; }
    }
}
