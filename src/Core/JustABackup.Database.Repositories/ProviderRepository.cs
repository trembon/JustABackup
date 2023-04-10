using JustABackup.Database.Contexts;
using JustABackup.Database.Entities;
using JustABackup.Database.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Database.Repositories
{
    public interface IProviderRepository
    {
        Task<Provider> Get(int id);

        Task<IEnumerable<Provider>> Get(ProviderType providerType);

        Task<ProviderInstance> GetInstance(int id);

        Task<int?> GetInstanceID(int jobId, int index);

        Task<IEnumerable<ProviderInstanceProperty>> GetProviderValues(int instanceId);
    }

    public class ProviderRepository : IProviderRepository
    {
        private DefaultContext context;

        public ProviderRepository(DefaultContext context)
        {
            this.context = context;
        }

        public async Task<Provider> Get(int id)
        {
            return await context
                .Providers
                .Include(p => p.Properties)
                .ThenInclude(pp => pp.Attributes)
                .FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<IEnumerable<Provider>> Get(ProviderType providerType)
        {
            return await context
                .Providers
                .Where(p => p.Type == providerType)
                .ToListAsync();
        }

        public async Task<ProviderInstance> GetInstance(int id)
        {
            ProviderInstance providerInstance = await context
                .ProviderInstances
                .Include(pi => pi.Provider)
                .Include(pi => pi.Values)
                .ThenInclude(pip => pip.Property)
                .ThenInclude(pp => pp.Attributes)
                .FirstOrDefaultAsync(pi => pi.ID == id);
            
            return providerInstance;
        }

        public async Task<int?> GetInstanceID(int jobId, int index)
        {
            if (jobId <= 0)
                return null;

            return await context
                .ProviderInstances
                .Where(pi => pi.Job.ID == jobId)
                .OrderBy(pi => pi.Order)
                .Select(pi => pi.ID)
                .Skip(index)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ProviderInstanceProperty>> GetProviderValues(int instanceId)
        {
            IEnumerable<ProviderInstanceProperty> providerInstanceProperties = await context
                .ProviderInstances
                .Where(pi => pi.ID == instanceId)
                .SelectMany(pi => pi.Values)
                .Include(v => v.Property)
                .ThenInclude(p => p.Attributes)
                .ToListAsync();
            
            return providerInstanceProperties;
        }
    }
}
