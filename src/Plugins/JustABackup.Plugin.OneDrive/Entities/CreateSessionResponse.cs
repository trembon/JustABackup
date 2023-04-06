using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Plugin.OneDrive.Entities
{
    public class CreateSessionResponse
    {
        [JsonProperty("uploadUrl")]
        public string UploadUrl { get; set; }

        [JsonProperty("expirationDateTime")]
        public DateTime ExpirationDateTime { get; set; }
    }
}
