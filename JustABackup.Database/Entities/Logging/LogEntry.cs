using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.Database.Entities.Logging
{
    public class LogEntry
    {
        [Key]
        [Required]
        public long ID { get; set; }

        [Required]
        public LogLevel Level { get; set; }

        public int EventID { get; set; }

        public string Category { get; set; }

        public string Message { get; set; }

        public DateTime Timestamp { get; set; }

        public List<LogException> Exceptions { get; set; }

        public LogEntry()
        {
            Timestamp = DateTime.Now;
            Exceptions = new List<LogException>();
        }
    }
}
