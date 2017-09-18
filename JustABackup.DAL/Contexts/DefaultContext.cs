using JustABackup.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.DAL.Contexts
{
    public class DefaultContext : DbContext
    {
        public DbSet<BackupJob> Jobs { get; set; }

        public DbSet<BackupJobHistory> JobHistory { get; set; }

        public DbSet<Provider> Providers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=demo.db");
        }
    }
}
