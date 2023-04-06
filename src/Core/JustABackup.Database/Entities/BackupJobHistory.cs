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

        [Required]
        public BackupJob Job { get; set; }

        public DateTime Started { get; set; }

        public DateTime Completed { get; set; } // TODO: make nullable

        public string Message { get; set; }

        public ExitCode Status { get; set; }

        public BackupJobHistory()
        {
            Status = ExitCode.NotCompleted;
        }
    }
}
