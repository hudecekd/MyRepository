using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace PushNotificationSenderWfa
{
    class Sender
    {
        private async Task<OAuthToken> GetAuthenticationTokenAsync(String packageSid, String clientSecret)
        {
            var content = new HttpFormUrlEncodedContent(
            new Dictionary<String, String> {
{ "grant_type", "client_credentials"},
{ "client_id", packageSid },
{ "client_secret", clientSecret},
{ "scope", "notify.windows.com"}
            });
            using (var response =
            await new HttpClient().PostAsync(new Uri("https://login.live.com/accesstoken.srf"), content).AsTask().ConfigureAwait(false))
            {
                return (OAuthToken)new DataContractJsonSerializer(typeof(OAuthToken))
                .ReadObject((await response.Content.ReadAsInputStreamAsync()
                .AsTask().ConfigureAwait(false)).AsStreamForRead());
            }
        }
    }
}
