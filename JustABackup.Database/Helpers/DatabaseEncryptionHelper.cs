using JustABackup.Database.Entities;
using JustABackup.Database.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Database.Helpers
{
    public interface IDatabaseEncryptionHelper
    {
        Task Decrypt(BackupJob backupJob);

        Task Decrypt(AuthenticatedSession authenticatedSession);

        Task Decrypt(IEnumerable<ProviderInstance> providerInstances);

        Task Decrypt(ProviderInstance providerInstance);

        Task Decrypt(IEnumerable<ProviderInstanceProperty> providerInstanceProperties);

        Task Decrypt(ProviderInstanceProperty providerInstanceProperty);


        Task Encrypt(BackupJob backupJob);

        Task Encrypt(AuthenticatedSession authenticatedSession);

        Task Encrypt(IEnumerable<ProviderInstance> providerInstances);

        Task Encrypt(ProviderInstance providerInstance);

        Task Encrypt(IEnumerable<ProviderInstanceProperty> providerInstanceProperties);

        Task Encrypt(ProviderInstanceProperty providerInstanceProperty);
    }

    public class DatabaseEncryptionHelper : IDatabaseEncryptionHelper
    {
        private string passphrase = null;

        private IConfiguration configuration;
        private IPassphraseRepository passphraseRepository;

        public DatabaseEncryptionHelper(IConfiguration configuration, IPassphraseRepository passphraseRepository)
        {
            this.configuration = configuration;
            this.passphraseRepository = passphraseRepository;
        }

        private async Task Initialize()
        {
            if (!string.IsNullOrWhiteSpace(passphrase))
                return;

            switch (configuration["encryption:storage"])
            {
                case "config":
                    passphrase = configuration["encryption:passphrase"];
                    break;

                case "environment":
                    passphrase = Environment.GetEnvironmentVariable(configuration["encryption:key"]);
                    break;
            }

            if (string.IsNullOrWhiteSpace(passphrase))
                passphrase = await passphraseRepository.GetOrAdd();
        }

        #region Decrypt
        public async Task Decrypt(BackupJob backupJob)
        {
            await Decrypt(backupJob?.Providers);
        }

        public async Task Decrypt(AuthenticatedSession authenticatedSession)
        {
            if (authenticatedSession == null)
                return;

            authenticatedSession.SessionData = await DecryptString(authenticatedSession.SessionData);
            await Decrypt(authenticatedSession.Provider);
        }

        public async Task Decrypt(IEnumerable<ProviderInstance> providerInstances)
        {
            await Decrypt(providerInstances?.SelectMany(pi => pi.Values));
        }

        public async Task Decrypt(ProviderInstance providerInstance)
        {
            await Decrypt(providerInstance?.Values);
        }

        public async Task Decrypt(IEnumerable<ProviderInstanceProperty> providerInstanceProperties)
        {
            if (providerInstanceProperties == null)
                return;

            foreach (ProviderInstanceProperty providerInstanceProperty in providerInstanceProperties)
                await Decrypt(providerInstanceProperty);
        }

        public async Task Decrypt(ProviderInstanceProperty providerInstanceProperty)
        {
            if (providerInstanceProperty == null)
                return;

            providerInstanceProperty.Value = await DecryptString(providerInstanceProperty.Value);
        }
        #endregion

        #region Encrypt
        public async Task Encrypt(BackupJob backupJob)
        {
            await Encrypt(backupJob?.Providers);
        }

        public async Task Encrypt(AuthenticatedSession authenticatedSession)
        {
            if (authenticatedSession == null)
                return;

            authenticatedSession.SessionData = await EncryptString(authenticatedSession.SessionData);
            await Encrypt(authenticatedSession.Provider);
        }

        public async Task Encrypt(IEnumerable<ProviderInstance> providerInstances)
        {
            await Encrypt(providerInstances?.SelectMany(pi => pi.Values));
        }

        public async Task Encrypt(ProviderInstance providerInstance)
        {
            await Encrypt(providerInstance?.Values);
        }

        public async Task Encrypt(IEnumerable<ProviderInstanceProperty> providerInstanceProperties)
        {
            if (providerInstanceProperties == null)
                return;

            foreach (ProviderInstanceProperty providerInstanceProperty in providerInstanceProperties)
                await Encrypt(providerInstanceProperty);
        }

        public async Task Encrypt(ProviderInstanceProperty providerInstanceProperty)
        {
            if (providerInstanceProperty == null)
                return;

            providerInstanceProperty.Value = await EncryptString(providerInstanceProperty.Value);
        }
        #endregion

        #region Logic
        // Source: https://stackoverflow.com/a/10177020

        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 128;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public async Task<string> EncryptString(string plainText)
        {
            await Initialize();

            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate128BitsOfRandomEntropy();
            var ivStringBytes = Generate128BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passphrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = Keysize;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;

                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();

                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();

                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public async Task<string> DecryptString(string cipherText)
        {
            await Initialize();

            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passphrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = Keysize;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;

                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate128BitsOfRandomEntropy()
        {
            var randomBytes = new byte[16]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
        #endregion
    }
}
