using JustABackup.Database.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Models
{
    public class RunHistoryItem
    {
        public int JobID { get; set; }

        public string JobName { get; set; }

        public DateTime Started { get; set; }

        public ExitCode Status { get; set; }

        public string Message { get; set; }
    }
}
