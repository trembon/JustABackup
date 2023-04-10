using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Services
{
    public static class DependencyInjectionExtensions
    {
        public static void AddJustABackupServices(this IServiceCollection services)
        {
            services.AddScoped<IInitializationService, InitializationService>();
            services.AddScoped<IProviderModelService, ProviderModelService>();
            services.AddScoped<IProviderMappingService, ProviderMappingService>();
            services.AddScoped<IEncryptionService, EncryptionService>();

            services.AddSingleton<ISchedulerService, SchedulerService>();
        }
    }
}
