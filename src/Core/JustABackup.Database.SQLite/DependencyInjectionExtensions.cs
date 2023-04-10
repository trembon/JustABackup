using JustABackup.Database.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Database.SQLite
{
    public static class DependencyInjectionExtensions
    {
        public static void AddSQLiteDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DefaultContext, DefaultSQLiteContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("Default"))
            );
        }
    }
}
