using JustABackup.Database.Entities.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Database
{
    public class LoggingContext : DbContext
    {
        public DbSet<LogEntry> Logs { get; set; }

        public LoggingContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
