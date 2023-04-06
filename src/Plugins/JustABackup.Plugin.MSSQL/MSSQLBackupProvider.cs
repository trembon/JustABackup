using JustABackup.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace JustABackup.Plugin.MSSQL
{
    [DisplayName("Microsoft SQL Backup")]
    public class MSSQLBackupProvider : IBackupProvider
    {
        public string Server { get; set; }

        public string Database { get; set; }

        public string Username { get; set; }

        [PasswordPropertyText]
        public string Password { get; set; }

        [Display(Name = "Temporary .bak file", Description = "Temporary file stored on the SQL server")]
        public string TemporaryBakFile { get; set; }


        private SqlConnection connection;
        private SqlCommand command;
        private SqlDataReader reader;
        private Stream stream;

        public Task<IEnumerable<BackupItem>> GetItems(DateTime? lastRun)
        {
            BackupItem database = new BackupItem($"{Database}.bak");
            IEnumerable<BackupItem> items = new BackupItem[] { database };

            return Task.FromResult(items);
        }

        public async Task<Stream> OpenRead(BackupItem item)
        {
            string connectionString = $"Server={Server};Database={Database};User Id={Username};Password={Password};";

            connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            command = new SqlCommand($"BACKUP DATABASE {Database} TO DISK = '{TemporaryBakFile}' WITH INIT, COPY_ONLY", connection);
            await command.ExecuteNonQueryAsync();
            
            command.CommandText = $"SELECT * FROM OPENROWSET(BULK N'{TemporaryBakFile}', SINGLE_BLOB) AS Contents";
            reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

            if (await reader.ReadAsync())
            {
                if (!(await reader.IsDBNullAsync(0)))
                {
                    stream = reader.GetStream(0);
                    return stream;
                }
            }

            throw new ArgumentOutOfRangeException("Database failed to backup or database not found.");
        }

        public void Dispose()
        {
            stream?.Dispose();
            reader?.Dispose();
            command?.Dispose();
            connection?.Dispose();
        }
    }
}
