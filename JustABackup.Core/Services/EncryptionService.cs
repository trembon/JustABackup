using JustABackup.Database.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.Services
{
    public interface IEncryptionService
    {
        Task<byte[]> Encrypt<T>(T data);

        Task<T> Decrypt<T>(byte[] data);
    }

    public class EncryptionService : IEncryptionService
    {
        // Source: https://stackoverflow.com/a/10177020
        
        private const int KEYSIZE = 128;
        private const int ITERATIONS = 1000;

        private string password;

        private IConfiguration configuration;
        private IPassphraseRepository passphraseRepository;

        public EncryptionService(IConfiguration configuration, IPassphraseRepository passphraseRepository)
        {
            this.configuration = configuration;
            this.passphraseRepository = passphraseRepository;
        }

        private async Task Initialize()
        {
            if (!string.IsNullOrWhiteSpace(password))
                return;
            
            switch (configuration["encryption:storage"])
            {
                case "config":
                    password = configuration["encryption:passphrase"];
                    break;

                case "environment":
                    password = Environment.GetEnvironmentVariable(configuration["encryption:key"]);
                    break;

                default:
                case "database":
                    password = await passphraseRepository.GetOrAdd();
                    break;
            }
        }

        public async Task<T> Decrypt<T>(byte[] data)
        {
            if (data == null || data.Length == 0)
                return default(T);

            await Initialize();

            // validate data in size, so it contains the header
            if (data.Length < (KEYSIZE / 8))
                throw new ArgumentOutOfRangeException(nameof(data));

            // get the salt from the 'header' and the object itself
            byte[] salt = data.Take(KEYSIZE / 8).ToArray();
            byte[] encryptedObject = data.Skip(salt.Length).ToArray();

            using (Rfc2898DeriveBytes encryptionKey = new Rfc2898DeriveBytes(password, salt, ITERATIONS))
            {
                var keyBytes = encryptionKey.GetBytes(KEYSIZE / 8);

                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = KEYSIZE;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, salt))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(encryptedObject))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                // decrypt the byte array
                                byte[] decryptedObject = new byte[encryptedObject.Length];
                                int decryptedByteCount = cryptoStream.Read(decryptedObject, 0, decryptedObject.Length);

                                // and deserialize the object
                                return DeserializeObject<T>(decryptedObject);
                            }
                        }
                    }
                }
            }
        }

        public async Task<byte[]> Encrypt<T>(T data)
        {
            if (data == null)
                return new byte[0];

            await Initialize();

            // serialize the object and generate a salt for the encryption
            byte[] dataBytes = SerializeObject(data);
            byte[] salt = GenerateRandomBytes();

            using (Rfc2898DeriveBytes encryptionKey = new Rfc2898DeriveBytes(password, salt, ITERATIONS))
            {
                var keyBytes = encryptionKey.GetBytes(KEYSIZE / 8);

                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = KEYSIZE;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, salt))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                // encrypt the object
                                cryptoStream.Write(dataBytes, 0, dataBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                
                                // merge the object with the salt as a 'header'
                                return salt.Concat(memoryStream.ToArray()).ToArray();
                            }
                        }
                    }
                }
            }
        }

        private byte[] SerializeObject(object data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, data);
                return ms.ToArray();
            }
        }

        private T DeserializeObject<T>(byte[] data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }

        private byte[] GenerateRandomBytes()
        {
            var randomBytes = new byte[KEYSIZE / 8];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
