using JustABackup.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Plugin.RijndaelEncryptTransformer
{
    public class RijndaelEncryptTransformer : ITransformProvider
    {
        public Task TransformItem(BackupItem item, Stream transformStream, Dictionary<BackupItem, Stream> inputFiles)
        {
            string password = @"myKey123";
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] key = UE.GetBytes(password);
            
            RijndaelManaged RMCrypto = new RijndaelManaged();

            CryptoStream cs = new CryptoStream(transformStream, RMCrypto.CreateEncryptor(key, key), CryptoStreamMode.Write);
            
            foreach(var input in inputFiles)
            {
                int data;
                while ((data = input.Value.ReadByte()) != -1)
                    cs.WriteByte((byte)data);
            }

            cs.FlushFinalBlock();

            return Task.CompletedTask;
        }

        public Task<IEnumerable<MappedBackupItem>> TransformList(IEnumerable<BackupItem> files)
        {
            List<MappedBackupItem> result = new List<MappedBackupItem>();

            foreach(var file in files)
            {
                BackupItem encryptedBackupItem = new BackupItem();
                encryptedBackupItem.Name = $"{file.Name}.enc";
                encryptedBackupItem.Path = file.Path;

                result.Add(new MappedBackupItem { Output = encryptedBackupItem, Input = new[] { file } });
            }

            return Task.FromResult((IEnumerable<MappedBackupItem>)result);
        }
    }
}
