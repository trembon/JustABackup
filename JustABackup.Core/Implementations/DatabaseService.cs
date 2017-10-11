using JustABackup.Core.Services;
using JustABackup.Core.Contexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Core.Implementations
{
    public class DatabaseService : IDatabaseService
    {
        private DefaultContext context;

        public DatabaseService(DefaultContext context)
        {
            this.context = context;
        }

        public void VerifyDatabase()
        {
            context.Database.EnsureCreated();
        }
    }
}
