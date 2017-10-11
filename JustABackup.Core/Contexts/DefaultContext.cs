using JustABackup.Core.Entities.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Core.Contexts
{
    public class DefaultContext : DbContext
    {
        internal DbSet<BackupJob> Jobs { get; set; }

        internal DbSet<BackupJobHistory> JobHistory { get; set; }

        internal DbSet<Provider> Providers { get; set; }

        public DefaultContext(DbContextOptions options) : base(options)
        {
        }
    }
}
