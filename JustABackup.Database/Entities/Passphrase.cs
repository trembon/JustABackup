using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.Database.Entities
{
    public class Passphrase
    {
        [Key]
        [Required]
        public int ID { get; set; }
        
        [Required]
        public string Key { get; set; }
    }
}
