using System;

namespace JustABackup.Models.Home
{
    public class JobHistoryModel
    {
        public int JobID { get; set; }

        public string JobName { get; set; }

        public DateTime Started { get; set; }

        public TimeSpan? RunTime { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }
    }
}
