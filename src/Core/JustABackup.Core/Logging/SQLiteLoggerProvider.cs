using JustABackup.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Core.Logging
{
    public class SQLiteLoggerProvider : ILoggerProvider
    {
        //private DbContextOptions<LoggingContext> dbContextOptions;
        private readonly ConcurrentDictionary<string, SQLiteLogger> loggers = new ConcurrentDictionary<string, SQLiteLogger>();

        //public SQLiteLoggerProvider(DbContextOptions<LoggingContext> dbContextOptions)
        //{
        //    this.dbContextOptions = dbContextOptions;

        //    using (LoggingContext db = new LoggingContext(dbContextOptions))
        //        db.Database.EnsureCreated();
        //}

        public ILogger CreateLogger(string categoryName)
        {
            return null;
            //return loggers.GetOrAdd(categoryName, name => new SQLiteLogger(name, dbContextOptions));
        }

        public void Dispose()
        {
            loggers.Clear();
        }
    }
}
