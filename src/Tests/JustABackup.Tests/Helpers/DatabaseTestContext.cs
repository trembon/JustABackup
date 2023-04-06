using JustABackup.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace JustABackup.Tests.Helpers
{
    public class DatabaseTestContext : DefaultContext
    {
        private DbConnection connection;

        public DatabaseTestContext(DbContextOptions options) : base(options)
        {
            connection = this.Database.GetDbConnection();
        }

        public override void Dispose()
        {
            base.Dispose();

            connection?.Close();
            connection?.Dispose();
        }
    }
}
