using JustABackup.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Tests.Helpers
{
    internal static class DatabaseHelper
    {
        public static DefaultContext CreateContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            
            var options = new DbContextOptionsBuilder<DatabaseTestContext>().UseSqlite(connection).Options;

            DatabaseTestContext context = new DatabaseTestContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
