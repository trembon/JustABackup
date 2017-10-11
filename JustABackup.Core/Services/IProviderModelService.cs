using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.Services
{
    public interface IProviderModelService
    {
        Task ProcessBackupProvider(Type type);

        Task ProcessStorageProvider(Type type);

        Task ProcessTransformProvider(Type type);
    }
}
