using JustABackup.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Core.Services
{
    public interface IRunHistoryRepository
    {
        IEnumerable<RunHistoryItem> GetLatestHistory(int maxCount);
    }
}
