using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Networking.PushNotifications;
using Windows.UI.Notifications;

namespace PushNotificationsBackgroundTask
{
    public sealed class PushNotificationsBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var rawNotification = taskInstance.TriggerDetails as RawNotification;
            var content = rawNotification.Content;

            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            // enables the tile to queue up to five notifications         
            updater.EnableNotificationQueue(true);
            updater.Clear();

            // get the XML content of one of the predefined tile templates, so that, you can customize it         
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText04);
            tileXml.GetElementsByTagName("text")[0].InnerText = (content.Length > 50 ? content.Substring(0, 50) : content);
            // Create a new tile notification.        

            var notification = new TileNotification(tileXml);
            notification.ExpirationTime = DateTimeOffset.Now.AddSeconds(10);
            updater.Update(notification);
        }
    }
}
