using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Core.Services
{
    public interface IInitializationService
    {
        void VerifyDatabase();
    }
}
