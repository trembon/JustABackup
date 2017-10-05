using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Core.Services
{
    public interface IProviderModelService
    {
        void ProcessBackupProvider(Type type);

        void ProcessStorageProvider(Type type);

        void ProcessTransformProvider(Type type);
    }
}
