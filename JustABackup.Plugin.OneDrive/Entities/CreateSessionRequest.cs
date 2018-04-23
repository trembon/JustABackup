using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Plugin.OneDrive.Entities
{
    public class CreateSessionRequest
    {
        public class SubItem
        {
            [JsonProperty("@microsoft.graph.conflictBehavior")]
            public string DuplicateAction { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        [JsonProperty("item")]
        public SubItem Item { get; set; }

        public CreateSessionRequest()
        {
            Item = new SubItem
            {
                DuplicateAction = "rename"
            };
        }

        public CreateSessionRequest(string name) : this()
        {
            this.Item.Name = name;
        }
    }
}
