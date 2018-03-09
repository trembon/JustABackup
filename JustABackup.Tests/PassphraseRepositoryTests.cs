using JustABackup.Database.Entities;
using JustABackup.Database.Repositories;
using JustABackup.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JustABackup.Tests
{
    public class PassphraseRepositoryTests
    {
        [Fact]
        public async void KeyExist()
        {
            using(var context = DatabaseHelper.CreateContext())
            {
                Passphrase passphrase = new Passphrase { Key = Guid.NewGuid().ToString() };
                context.Passphrase.Add(passphrase);
                context.SaveChanges();

                var repo = new PassphraseRepository(context);
                string key = await repo.GetOrAdd();

                Assert.Equal(passphrase.Key, key);
            }
        }

        [Fact]
        public async void KeyDoesNotExist()
        {
            using (var context = DatabaseHelper.CreateContext())
            {
                var repo = new PassphraseRepository(context);
                string key = await repo.GetOrAdd();

                Assert.True(Guid.TryParse(key, out Guid result));
            }
        }
    }
}
