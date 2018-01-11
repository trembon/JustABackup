using JustABackup.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Database.Repositories
{
    public interface IAuthenticatedSessionRepository
    {
        Task<Dictionary<int, string>> GetAuthenticatedSessions(string type);
    }

    public class AuthenticatedSessionRepository : IAuthenticatedSessionRepository
    {
        private DefaultContext context;

        public AuthenticatedSessionRepository(DefaultContext context)
        {
            this.context = context;
        }

        public async Task<Dictionary<int, string>> GetAuthenticatedSessions(string type)
        {
            return await context
                .AuthenticatedSessions
                .Include(a => a.Provider)
                .ThenInclude(p => p.Provider)
                .Where(a => a.Provider.Provider.GenericType == type)
                .ToDictionaryAsync(k => k.ID, v => v.Name);
        }
    }
}
