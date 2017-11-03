using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.Services
{
    public interface IInitializationService
    {
        Task VerifyDatabase();

        Task VerifyScheduledJobs();

        Task LoadPlugins();
    }
}
