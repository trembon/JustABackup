using JustABackup.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using JustABackup.Database;

namespace JustABackup.Core.Implementations
{
    public class InitializationService : IInitializationService
    {
        private DefaultContext context;

        public InitializationService(DefaultContext context)
        {
            this.context = context;
        }

        public void VerifyDatabase()
        {
            context.Database.EnsureCreated();
        }
    }
}
