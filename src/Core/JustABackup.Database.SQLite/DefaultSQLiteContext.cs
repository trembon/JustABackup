using JustABackup.Database.Contexts;
using Microsoft.EntityFrameworkCore;

namespace JustABackup.Database.SQLite
{
    public class DefaultSQLiteContext : DefaultContext
    {
        public DefaultSQLiteContext(DbContextOptions<DefaultSQLiteContext> options) : base(options)
        {
        }
    }
}
