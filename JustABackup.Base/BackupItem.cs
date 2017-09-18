using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JustABackup.Base
{
    public class BackupItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public virtual Stream OpenRead()
        {
            return new MemoryStream();
        }
    }
}
