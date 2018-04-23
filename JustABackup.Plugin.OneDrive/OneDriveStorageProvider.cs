using JustABackup.Base;
using JustABackup.Base.Attributes;
using JustABackup.Plugin.OneDrive.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Plugin.OneDrive
{
    [DisplayName("OneDrive Storage")]
    public class OneDriveStorageProvider : IStorageProvider
    {
        private const string API_URL = "https://api.onedrive.com/v1.0/drive/root:{0}:/upload.createSession";

        [Transform]
        public string Folder { get; set; }

        public IAuthenticatedClient<OneDriveClient> Client { get; set; }

        public async Task<bool> StoreItem(BackupItem item, Stream source)
        {
            string path = Path.Combine(Folder, item.FullPath);
            if (item.FullPath.StartsWith("/"))
                path = Path.Combine(Folder, item.FullPath.Substring(1));

            var client = await Client.GetClient();

            using (HttpClient httpClient = new HttpClient())
            {
                CreateSessionResponse createSessionResponse = null;
                try
                {
                    CreateSessionRequest createSession = new CreateSessionRequest(item.Name);

                    HttpRequestMessage createSessionHttpRequest = new HttpRequestMessage(HttpMethod.Post, string.Format(API_URL, path));
                    createSessionHttpRequest.Content = new StringContent(JsonConvert.SerializeObject(createSession), Encoding.UTF8, "application/json");

                    await client.AuthenticateRequestAsync(createSessionHttpRequest);

                    HttpResponseMessage createSessionHttpResponse = await httpClient.SendAsync(createSessionHttpRequest);
                    string createSessionHttpResponseString = await createSessionHttpResponse.Content.ReadAsStringAsync();

                    createSessionResponse = JsonConvert.DeserializeObject<CreateSessionResponse>(createSessionHttpResponseString);
                }
                catch (Exception)
                {
                    // TODO: logging in providers
                    return false;
                }

                try
                {
                    int readSize = 0;
                    byte[] buffer = new byte[327680 * 6];

                    while ((readSize = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        HttpRequestMessage uploadRequest = new HttpRequestMessage(HttpMethod.Put, createSessionResponse.UploadUrl);
                        uploadRequest.Content = new ByteArrayContent(buffer, 0, readSize);
                        uploadRequest.Content.Headers.Add("Content-Range", $"bytes {source.Position - readSize}-{source.Position - 1}/{source.Length}");

                        await client.AuthenticateRequestAsync(uploadRequest);

                        HttpResponseMessage uploadResponse = await httpClient.SendAsync(uploadRequest);
                        if (uploadResponse.StatusCode != HttpStatusCode.Accepted && uploadResponse.StatusCode != HttpStatusCode.Created)
                            throw new Exception(await uploadResponse.Content.ReadAsStringAsync());
                    }
                }
                catch (Exception)
                {
                    // TODO: send cleanup cmd to onedrive (https://docs.microsoft.com/en-us/onedrive/developer/rest-api/api/driveitem_createuploadsession)

                    // TODO: logging in providers
                    return false;
                }
            }

            return true;
        }

        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}
