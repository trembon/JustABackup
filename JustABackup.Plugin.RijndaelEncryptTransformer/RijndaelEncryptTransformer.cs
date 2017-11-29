using JustABackup.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Plugin.RijndaelEncryptTransformer
{
    public class RijndaelEncryptTransformer : ITransformProvider
    {
        [Display(Name = "Encryption key")]
        public string EncyptionKey { get; set; }

        public async Task TransformItem(BackupItem output, Stream outputStream, Dictionary<BackupItem, Stream> inputFiles)
        {
            byte[] key = Encoding.Unicode.GetBytes(this.EncyptionKey);

            using (RijndaelManaged rmCrypto = new RijndaelManaged())
            {
                CryptoStream cs = new CryptoStream(outputStream, rmCrypto.CreateEncryptor(key, key), CryptoStreamMode.Write);

                foreach (var input in inputFiles)
                    await input.Value.CopyToAsync(cs);

                cs.FlushFinalBlock();
            }
        }

        public Task<MappedBackupItemList> MapInput(IEnumerable<BackupItem> input)
{
            MappedBackupItemList result = new MappedBackupItemList();

            foreach(var file in input)
            {
                BackupItem encryptedBackupItem = new BackupItem();
                encryptedBackupItem.Name = $"{file.Name}.dat";
                encryptedBackupItem.Path = file.Path;

                result.Add(encryptedBackupItem, file);
            }

            return Task.FromResult(result);
        }
    }
}
