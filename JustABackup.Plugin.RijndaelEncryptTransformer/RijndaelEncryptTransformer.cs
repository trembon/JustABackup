using JustABackup.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Plugin.RijndaelEncryptTransformer
{
    [DisplayName("Encryption Transformer (Rijndael)")]
    public class RijndaelEncryptTransformer : ITransformProvider
    {
        [PasswordPropertyText]
        [Display(Name = "Encryption key")]
        public string EncyptionKey { get; set; }

        private const int KEYSIZE = 128;
        private const int ITERATIONS = 1000;

        private List<Stream> streams;
        
        public RijndaelEncryptTransformer()
        {
            streams = new List<Stream>();
        }

        public async Task TransformItem(BackupItem output, Stream outputStream, Dictionary<BackupItem, Stream> inputFiles)
        {
            byte[] password = Encoding.Unicode.GetBytes(EncyptionKey);
            byte[] salt = GenerateRandomBytes();

            using (Rfc2898DeriveBytes encryptionKey = new Rfc2898DeriveBytes(password, salt, ITERATIONS))
            {
                var keyBytes = encryptionKey.GetBytes(KEYSIZE / 8);

                using (RijndaelManaged rmCrypto = new RijndaelManaged())
                {
                    rmCrypto.BlockSize = KEYSIZE;
                    rmCrypto.Mode = CipherMode.CBC;
                    rmCrypto.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform encryptor = rmCrypto.CreateEncryptor(keyBytes, salt))
                    {
                        outputStream.Write(salt, 0, salt.Length);

                        CryptoStream cs = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write);

                        foreach (var input in inputFiles)
                            await input.Value.CopyToAsync(cs);

                        cs.FlushFinalBlock();
                        streams.Add(cs);
                    }
                }
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

        public Task<MappedBackupItemList> MapInput(IEnumerable<BackupItem> input)
        {
            MappedBackupItemList result = new MappedBackupItemList();

            foreach (var file in input)
            {
                BackupItem encryptedBackupItem = new BackupItem();
                encryptedBackupItem.Name = $"{file.Name}.dat";
                encryptedBackupItem.Path = file.Path;

                result.Add(encryptedBackupItem, file);
            }

            return Task.FromResult(result);
        }

        public void Dispose()
        {
            streams.ForEach(s => s.Dispose());
        }
    }
}
