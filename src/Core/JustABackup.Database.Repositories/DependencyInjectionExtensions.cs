using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Database.Repositories
{
    public static class DependencyInjectionExtensions
    {
        public static void AddJustABackupRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticatedSessionRepository, AuthenticatedSessionRepository>();
            services.AddScoped<IBackupJobRepository, BackupJobRepository>();
            services.AddScoped<IPassphraseRepository, PassphraseRepository>();
            services.AddScoped<IProviderRepository, ProviderRepository>();
        }
    }
}
