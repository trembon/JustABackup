using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Models.Provider
{
    public class ProviderFieldModel
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public string Type { get; set; }

        public string[] Validation { get; set; }

        public string DataSource { get; set; }
    }
}
