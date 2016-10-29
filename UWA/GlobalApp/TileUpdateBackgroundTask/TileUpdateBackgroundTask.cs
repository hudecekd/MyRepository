using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace TileUpdateBackgroundTask
{
    public sealed class TileUpdateBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            UpdateTile();
        }

        public static void UpdateTile()
        {         // create the instance of Tile Updater, which enables you to change the appearance of the calling app's tile         
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            // enables the tile to queue up to five notifications         
            updater.EnableNotificationQueue(true);
            updater.Clear();

            // task is set to run after 15 minutes so we need to schedule 15 notifications
            // to change tile every minute
            // WARNING: Maybe there can be a gap which will require more then 15 notifications???

            var now = DateTime.Now;

            for (int i = 1; i <= 15; i++)
            {
                var notifyTime = now.Subtract(TimeSpan.FromSeconds(now.Second)).AddMinutes(i); // plan it for start of the new minute
                var notifyText = notifyTime.ToString("MM.dd.yyyy hh:mm", System.Globalization.CultureInfo.InvariantCulture);

                // get the XML content of one of the predefined tile templates, so that, you can customize it         
                XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText04);
                tileXml.GetElementsByTagName("text")[0].InnerText = notifyText;
                // Create a new tile notification.        

                var notification = new ScheduledTileNotification(tileXml, notifyTime);
                notification.ExpirationTime = notifyTime.AddSeconds(10);

                updater.AddToSchedule(notification);
            }
        }
    }
}
