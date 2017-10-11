using JustABackup.Core.Contexts;
using JustABackup.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.Implementations
{
    public class ProviderRepository : IProviderRepository
    {
        private DefaultContext defaultContext;

        public ProviderRepository(DefaultContext defaultContext)
        {
            this.defaultContext = defaultContext;
        }

        public async Task InvalidateExistingProviders()
        {
            var providers = defaultContext.Providers;
            foreach (var provider in providers)
                provider.IsProcessed = false;

            await defaultContext.SaveChangesAsync();
        }
    }
}
