using JustABackup.Base;
using JustABackup.Core.Services;
using JustABackup.Database;
using JustABackup.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.Entities
{
    public class AuthenticatedClient<T> : IAuthenticatedClient<T> where T : class
    {
        private IProviderMappingService providerMappingService;
        private DefaultContext dbContext;

        public long ID { get; }

        public AuthenticatedClient(long id)
        {
            this.ID = id;
        }

        public AuthenticatedClient(long id, IProviderMappingService providerMappingService, DefaultContext dbContext)
            : this(id)
        {
            this.providerMappingService = providerMappingService;
            this.dbContext = dbContext;
        }

        public async Task<T> GetClient()
        {
            AuthenticatedSession session = await dbContext
                .AuthenticatedSessions
                .Include(a => a.Provider)
                .FirstOrDefaultAsync(a => a.ID == ID);

            IAuthenticationProvider<T> authenticationProvider = await providerMappingService?.CreateProvider<IAuthenticationProvider<T>>(session.Provider.ID);
            return authenticationProvider.GetAuthenticatedClient(session.SessionData);
        }

        public override string ToString()
        {
            return ID.ToString();
        }
    }
}
