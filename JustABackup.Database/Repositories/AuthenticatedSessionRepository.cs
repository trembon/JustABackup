using JustABackup.Database;
using JustABackup.Database.Entities;
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
        Task<IEnumerable<AuthenticatedSession>> Get();

        Task<AuthenticatedSession> Get(int id);

        Task<IEnumerable<AuthenticatedSession>> GetAll(string type);

        Task<int> Add(string name, byte[] sessionData, ProviderInstance providerInstance);

        Task<bool> StoreSession(int sessionId, byte[] sessionData);
    }

    public class AuthenticatedSessionRepository : IAuthenticatedSessionRepository
    {
        private DefaultContext context;

        public AuthenticatedSessionRepository(DefaultContext context)
        {
            this.context = context;
        }

        public async Task<int> Add(string name, byte[] sessionData, ProviderInstance providerInstance)
        {
            AuthenticatedSession session = new AuthenticatedSession();
            session.Name = name;
            session.SessionData = sessionData;
            session.Provider = providerInstance;

            await context.AuthenticatedSessions.AddAsync(session);
            await context.SaveChangesAsync();

            return session.ID;
        }
        
        public async Task<IEnumerable<AuthenticatedSession>> GetAll(string type)
        {
            return await context
                .AuthenticatedSessions
                .Include(a => a.Provider)
                .ThenInclude(p => p.Provider)
                .Where(a => a.Provider.Provider.GenericType == type)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuthenticatedSession>> Get()
        {
            return await context
                .AuthenticatedSessions
                .Include(api => api.Provider)
                .ThenInclude(pi => pi.Provider)
                .OrderBy(api => api.Name)
                .ToListAsync();
        }

        public async Task<bool> StoreSession(int sessionId, byte[] sessionData)
        {
            AuthenticatedSession session = await context.AuthenticatedSessions.FirstOrDefaultAsync(api => api.ID == sessionId);
            if (session != null)
            {
                session.SessionData = sessionData;

                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<AuthenticatedSession> Get(int id)
        {
            AuthenticatedSession session = await context
                .AuthenticatedSessions
                .Include(a => a.Provider)
                .FirstOrDefaultAsync(a => a.ID == id);
            
            return session;
        }
    }
}
