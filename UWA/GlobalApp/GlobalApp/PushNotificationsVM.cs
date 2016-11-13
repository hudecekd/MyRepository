using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;

namespace GlobalApp
{
    public class PushNotificationsVM
    {
        public static ObservableCollection<PushNotificationVM> Notifications { get; set; } = new ObservableCollection<PushNotificationVM>();

        public static volatile bool PassToBackgroundTask = true;

        public static volatile string ChannelUri;
    }

    public class PushNotificationVM
    {
        public DateTime DateTime { get; set; }
        public string Notification { get; set; }
        public PushNotificationType Type { get; set; }


        /// <summary>
        /// Because UWA does not have StringFormat in binding.
        /// </summary>
        public string DateTimeText
        {
            get
            {
                return DateTime.ToString("MM.dd.yyyy hh:mm:ss");
            }
        }
    }
}
