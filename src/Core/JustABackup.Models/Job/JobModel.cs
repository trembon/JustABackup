using System;

namespace JustABackup.Models.Job
{
    public class JobModel
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public bool HasChangedModel { get; set; }

        public DateTime? NextRun { get; set; }

        public DateTime? LastRun { get; set; }
    }
}