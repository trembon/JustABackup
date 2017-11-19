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

        public string FullPath => $"{Path}{Name}";
        
        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            var item = obj as BackupItem;
            return item != null &&
                   Name == item.Name &&
                   Path == item.Path;
        }

        public override int GetHashCode()
        {
            var hashCode = 193482316;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
            return hashCode;
        }
    }
}
