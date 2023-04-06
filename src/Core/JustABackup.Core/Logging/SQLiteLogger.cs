using JustABackup.Database;
using JustABackup.Database.Entities.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Core.Logging
{
    public class SQLiteLogger : ILogger
    {
        private string categoryName;
        private DbContextOptions<LoggingContext> dbContextOptions;

        public SQLiteLogger(string categoryName, DbContextOptions<LoggingContext> dbContextOptions)
        {
            this.categoryName = categoryName;
            this.dbContextOptions = dbContextOptions;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                using (LoggingContext db = new LoggingContext(dbContextOptions))
                {
                    LogEntry entry = new LogEntry();
                    entry.Level = logLevel;
                    entry.Category = categoryName;
                    entry.EventID = eventId.Id;
                    entry.Message = formatter(state, exception);

                    Exception ex = exception;
                    for (int i = 0; ex != null; i++)
                    {
                        LogException logException = new LogException();
                        logException.Order = i;
                        logException.Type = ex.GetType().FullName;
                        logException.Message = ex.Message;
                        logException.StackTrace = ex.StackTrace;
                        logException.Source = ex.Source;
                        logException.HelpLink = ex.HelpLink;

                        entry.Exceptions.Add(logException);

                        ex = ex.InnerException;
                    }

                    db.Add(entry);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} - ERROR: Failed to add log entry to database. ({ex.GetType().FullName}: {ex.Message})");
            }
        }
    }
}
