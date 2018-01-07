using JustABackup.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.Repositories
{
    public interface IAuthenticatedSessionRepository
    {
        Task<Dictionary<int, string>> GetAuthenticatedSessions(string type);
    }

    public class AuthenticatedSessionRepository : IAuthenticatedSessionRepository
    {
        private DefaultContext dbContext;

        public AuthenticatedSessionRepository(DefaultContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Dictionary<int, string>> GetAuthenticatedSessions(string type)
        {
            return await dbContext
                .AuthenticatedSessions
                .Include(a => a.Provider)
                .ThenInclude(p => p.Provider)
                .Where(a => a.Provider.Provider.GenericType == type)
                .ToDictionaryAsync(k => k.ID, v => v.Name);
        }
    }
}
