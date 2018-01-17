using JustABackup.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Database.Repositories
{
    public interface IPassphraseRepository
    {
        Task<string> GetOrAdd();
    }

    public class PassphraseRepository : IPassphraseRepository
    {
        private DefaultContext context;

        public PassphraseRepository(DefaultContext context)
        {
            this.context = context;
        }

        public async Task<string> GetOrAdd()
        {
            Passphrase passphrase = await context.Passphrase.FirstOrDefaultAsync();
            if(passphrase == null)
            {
                passphrase = new Passphrase();
                passphrase.Key = Guid.NewGuid().ToString();

                await context.Passphrase.AddAsync(passphrase);
                await context.SaveChangesAsync();
            }

            return passphrase.Key;
        }
    }
}
