using JustABackup.Base;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace JustABackup.Plugin.MySQL
{
    public class MySQLBackupProvider : IBackupProvider
    {
        public string Server { get; set; }

        public string Database { get; set; }

        public string Username { get; set; }

        [PasswordPropertyText]
        public string Password { get; set; }


        private MemoryStream memoryStream;

        public Task<IEnumerable<BackupItem>> GetItems(DateTime? lastRun)
        {
            BackupItem database = new BackupItem($"{Database}.sql");
            IEnumerable<BackupItem> items = new BackupItem[] { database };

            return Task.FromResult(items);
        }

        public Task<Stream> OpenRead(BackupItem item)
        {
            string constring = $"server={Server};user={Username};pwd={Password};database={Database};SslMode=none;";

            // Important Additional Connection Options
            constring += "charset=utf8;convertzerodatetime=true;";
            
            memoryStream = new MemoryStream();

            using (MySqlConnection conn = new MySqlConnection(constring))
            {
                using (MySqlCommand cmd = new MySqlCommand { Connection = conn })
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        conn.Open();
                        mb.ExportToMemoryStream(memoryStream);
                    }
                }
            }

            memoryStream.Seek(0, SeekOrigin.Begin);

            return Task.FromResult(memoryStream as Stream);
        }

        public void Dispose()
        {
            memoryStream?.Dispose();
        }
    }
}
