using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.Database.Entities
{
    public class LogException
    {
        [Key]
        [Required]
        public long ID { get; set; }

        [Required]
        public LogEntry Entry { get; set; }

        public int Order { get; set; }

        public string Type { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public string Source { get; set; }

        public string HelpLink { get; set; }
    }
}
