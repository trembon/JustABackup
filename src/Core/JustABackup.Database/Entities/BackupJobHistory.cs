﻿using JustABackup.Database.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.Database.Entities
{
    public class BackupJobHistory
    {
        [Key]
        [Required]
        public int ID { get; set; }

        public BackupJob Job { get; set; }

        [Required]
        public int JobID { get; set; }

        public DateTime Started { get; set; }

        public DateTime? Completed { get; set; }

        public string Message { get; set; }

        public ExitCode Status { get; set; }

        public BackupJobHistory()
        {
            Status = ExitCode.NotCompleted;
        }
    }
}
