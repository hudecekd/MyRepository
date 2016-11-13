using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PushNotificationSenderApp
{
    public enum NotificationType
    {
        Tile,
        Badge,
        Toast,
        Raw
    }

    public class TypeMV
    {
        public string Text { get; set; }
        public NotificationType Type { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            cmbType.Items.Add(new TypeMV() { Text = "Toast", Type = NotificationType.Toast });
            cmbType.Items.Add(new TypeMV() { Text = "Badge", Type = NotificationType.Badge });
            cmbType.Items.Add(new TypeMV() { Text = "Raw", Type = NotificationType.Raw });
            cmbType.Items.Add(new TypeMV() { Text = "Tile", Type = NotificationType.Tile });
        }

        private async Task<OAuthToken> GetAuthenticationTokenAsync(String packageSid, String clientSecret)
        {
            var content = new Windows.Web.Http.HttpFormUrlEncodedContent(
            new Dictionary<String, String> {
{ "grant_type", "client_credentials"},
{ "client_id", packageSid },
{ "client_secret", clientSecret},
{ "scope", "notify.windows.com"}
            });
            using (var response =
            await new HttpClient()
            .PostAsync(new Uri("https://login.live.com/accesstoken.srf"), content)
            .AsTask().ConfigureAwait(false))
            {
                return (OAuthToken)new DataContractJsonSerializer(typeof(OAuthToken))
                .ReadObject((await response.Content.ReadAsInputStreamAsync()
                .AsTask().ConfigureAwait(false)).AsStreamForRead());
            }
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            var type = (NotificationType)cmbType.SelectedValue;

            var clientSecret = "jwD2/Hd0P1MPbnFeYr2OXQGs9BwkuZpJ";
            var sid = "ms-app://s-1-15-2-2835592131-1218219412-2347477943-2534017479-300382326-1664356857-1512323409";
            var token = await GetAuthenticationTokenAsync(sid, clientSecret);

            HttpResponseMessage response;

            switch (type)
            {
                case NotificationType.Raw:
                    response = await PushRawAsync(txtPayload.Text, txtChannelUri.Text, token);
                    break;
                case NotificationType.Toast:
                    // use xml instead of text
                    response = await PushToastAsync(txtPayload.Text, txtChannelUri.Text, token);
                    break;
            }
        }

        public async Task<HttpResponseMessage> PushRawAsync(string payload, string channelUri, OAuthToken token)
        {
            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(txtPayload.Text);
            return await PushAsync(NotificationType.Raw, payloadBytes, channelUri, token);
        }

        public async Task<HttpResponseMessage> PushToastAsync(string xmlPayload, string channelUri, OAuthToken token)
        {
            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(txtPayload.Text);
            return await PushAsync(NotificationType.Toast, payloadBytes, channelUri, token);
        }

        private string GetRequestType(NotificationType type)
        {
            if (type == NotificationType.Badge) return "badge";
            if (type == NotificationType.Raw) return "raw";
            if (type == NotificationType.Tile) return "tile";
            if (type == NotificationType.Toast) return "toast";
            throw new InvalidOperationException("Unknown notification type");
        }


        public async Task<HttpResponseMessage> PushAsync(
            NotificationType type,
Byte[] payload, String channelUri, OAuthToken token)
        {
            var requestNotificationType = GetRequestType(type);

            var contentType = "";
            if (type == NotificationType.Raw) contentType = "application/octet-stream";
            if (type == NotificationType.Toast) contentType = "text/xml";

            var msg = new HttpRequestMessage(HttpMethod.Post, new Uri(channelUri));
            // Set mandatory information:
            msg.Headers.Authorization = new HttpCredentialsHeaderValue("Bearer", token.AccessToken);
            msg.Content = new HttpBufferContent(payload.AsBuffer());
            msg.Headers.Add("X-WNS-Type", "wns/" + requestNotificationType);

            // For "wns/raw", the content type must be "application/octet-stream"
            //msg.Content.Headers.ContentType = new HttpMediaTypeHeaderValue("text/xml");
            msg.Content.Headers.ContentType = new HttpMediaTypeHeaderValue(contentType);

            msg.Content.Headers.ContentLength = payload.AsBuffer().Length;
            
            // Set optional headers:
            msg.Headers.Add("X-WNS-Cache-Policy", "cache");
            msg.Headers.Add("X-WNS-TTL", "60"); // Seconds
                                                // Assign tag label (max 16 chars) to notification; used by device to detect dups.

            
            if ((type == NotificationType.Tile) || (type == NotificationType.Toast))
            {
                msg.Headers.Add("X-WNS-Tag", "SomeTag");
            }
            // Request for Device Status and Notification Status to be returned in the response
            msg.Headers.Add("X-WNS-RequestForStatus", "true");
            return await new HttpClient().SendRequestAsync(msg).AsTask().ConfigureAwait(false);
        }
    }

    [DataContract]
    public sealed class OAuthToken
    {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }
        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }
    }
}
