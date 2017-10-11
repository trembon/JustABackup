using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.Services
{
    public interface IProviderRepository
    {
        Task InvalidateExistingProviders();
    }
}
