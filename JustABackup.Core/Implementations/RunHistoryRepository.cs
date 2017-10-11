using JustABackup.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using JustABackup.Core.Entities;
using JustABackup.Core.Contexts;
using System.Linq;

namespace JustABackup.Core.Implementations
{
    public class RunHistoryRepository : IRunHistoryRepository
    {
        private DefaultContext defaultContext;

        public RunHistoryRepository(DefaultContext defaultContext)
        {
            this.defaultContext = defaultContext;
        }

        public IEnumerable<RunHistoryItem> GetLatestHistory(int maxCount)
        {
            return defaultContext
                .JobHistory
                .OrderByDescending(jh => jh.Started)
                .Take(maxCount)
                .Select(jh => new RunHistoryItem
                {
                    JobID = jh.Job.ID,
                    JobName = jh.Job.Name,
                    Started = jh.Started,
                    Status = jh.Status,
                    Message = jh.Message
                });
        }
    }
}
