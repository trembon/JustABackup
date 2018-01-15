using JustABackup.Base;
using JustABackup.Core.Services;
using JustABackup.Database;
using JustABackup.Database.Entities;
using JustABackup.Database.Repositories;
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
        private IAuthenticatedSessionRepository authenticatedSessionRepository;

        public int ID { get; }

        public AuthenticatedClient(int id)
        {
            this.ID = id;
        }

        public AuthenticatedClient(int id, IProviderMappingService providerMappingService, IAuthenticatedSessionRepository authenticatedSessionRepository)
            : this(id)
        {
            this.providerMappingService = providerMappingService;
            this.authenticatedSessionRepository = authenticatedSessionRepository;
        }

        public async Task<T> GetClient()
        {
            // TODO: place in service?
            AuthenticatedSession session = await authenticatedSessionRepository.Get(ID);

            IAuthenticationProvider<T> authenticationProvider = await providerMappingService?.CreateProvider<IAuthenticationProvider<T>>(session.Provider.ID);
            return authenticationProvider.GetAuthenticatedClient(session.SessionData);
        }

        public override string ToString()
        {
            return ID.ToString();
        }
    }
}
