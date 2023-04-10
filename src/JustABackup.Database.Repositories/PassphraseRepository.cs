using JustABackup.Database.Contexts;
using JustABackup.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace JustABackup.Database.Repositories
{
    public interface IPassphraseRepository
    {
        Task<string> GetOrAdd();
    }

    public class PassphraseRepository : IPassphraseRepository
    {
        private readonly DefaultContext context;

        public PassphraseRepository(DefaultContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<string> GetOrAdd()
        {
            Passphrase? passphrase = await context.Passphrase.FirstOrDefaultAsync();
            if(passphrase == null)
            {
                passphrase = new Passphrase
                {
                    Key = Guid.NewGuid().ToString()
                };

                await context.Passphrase.AddAsync(passphrase);
                await context.SaveChangesAsync();
            }

            return passphrase.Key;
        }
    }
}
