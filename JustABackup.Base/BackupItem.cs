using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JustABackup.Base
{
    /// <summary>
    /// Class to represent a file that should be backed up.
    /// </summary>
    public class BackupItem
    {
        /// <summary>
        /// Name of the file.
        /// Example: file1.txt
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The path of folders to the file.
        /// Example: /path/to/a/folder
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The full path of the file.
        /// Example: /path/to/a/folder/file1.txt
        /// </summary>
        public string FullPath
        {
            get
            {
                return System.IO.Path.Combine(Path, Name);
            }
        }

        public BackupItem()
        {
        }

        public BackupItem(string name)
            : this(name, "/")
        {
        }

        public BackupItem(string name, string path)
        {
            this.Name = name;
            this.Path = path;
        }

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
