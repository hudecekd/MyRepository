using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GlobalApp
{
    public sealed partial class AlarmSettingUC : UserControl
    {
        public AlarmSettingUC()
        {
            this.InitializeComponent();

            var sounds = new List<string>() {
"ms-appdata:///local/piano.wav",
"ms-appdata:///local/01 - My Silver Lining (4).mp3",


"ms-winsoundevent:Notification.Default",
"ms-winsoundevent:Notification.IM",
"ms-winsoundevent:Notification.Mail",
"ms-winsoundevent:Notification.Reminder",
"ms-winsoundevent:Notification.SMS",
"ms-winsoundevent:Notification.Looping.Alarm",
"ms-winsoundevent:Notification.Looping.Alarm2",
"ms-winsoundevent:Notification.Looping.Alarm3",
"ms-winsoundevent:Notification.Looping.Alarm4",
"ms-winsoundevent:Notification.Looping.Alarm5",
"ms-winsoundevent:Notification.Looping.Alarm6",
"ms-winsoundevent:Notification.Looping.Alarm7",
"ms-winsoundevent:Notification.Looping.Alarm8",
"ms-winsoundevent:Notification.Looping.Alarm9",
"ms-winsoundevent:Notification.Looping.Alarm10",
"ms-winsoundevent:Notification.Looping.Call",
"ms-winsoundevent:Notification.Looping.Call2",
"ms-winsoundevent:Notification.Looping.Call3",
"ms-winsoundevent:Notification.Looping.Call4",
"ms-winsoundevent:Notification.Looping.Call5",
"ms-winsoundevent:Notification.Looping.Call6",
"ms-winsoundevent:Notification.Looping.Call7",
"ms-winsoundevent:Notification.Looping.Call8",
"ms-winsoundevent:Notification.Looping.Call9",
"ms-winsoundevent:Notification.Looping.Call10"};

            foreach (var sound in sounds)
            {
                cmbSounds.Items.Add(sound);
            }
        }

        private string toastXml =
@"
<toast duration=""{2}"" scenario=""alarm"" launch=""app-defined-string"">
  <visual>
    <binding template=""ToastGeneric"" >
      <text>Sample</text>
      <text>This is a simple toast notification example</text>
      <image placement=""AppLogoOverride"" src=""oneAlarm.png"" />
    </binding>
  </visual>
  <actions>
    <action content=""check"" arguments=""check"" imageUri=""check.png"" />
    <action content= ""cancel"" arguments=""cancel"" />
    <action activationType=""system"" arguments=""dismiss"" content="""" />
  </actions>
  <audio  src=""{0}"" loop=""{1}""/>
</toast>
";

        private void btnCreateNotification_Click(object sender, RoutedEventArgs e)
        {
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();

            var durationStr = (chbDuration.IsChecked.HasValue && chbDuration.IsChecked.Value) ? "long" : "short";
            var loopStr = (chbLoop.IsChecked.HasValue && chbLoop.IsChecked.Value ? "true":  "false");
            var xmlString = string.Format(toastXml, cmbSounds.SelectedItem, loopStr, durationStr);

            var xml = new XmlDocument();
            xml.LoadXml(xmlString);

            var dateTime = dpDate.Date.Date.Add(tpTime.Time).ToUniversalTime();

            //var toast = new ScheduledToastNotification(xml, new DateTimeOffset(DateTime.Now.AddSeconds(10).ToUniversalTime()));
            var toast = new ScheduledToastNotification(xml, dateTime);

            ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
        }

        private async void btnCopyAudio_Click(object sender, RoutedEventArgs e)
        {
            var files = new List<string>()
            {
                "ms-appx:///Assets/piano.wav",
                "ms-appx:///Assets/01 - My Silver Lining (4).mp3"
            };

            foreach (var file in files)
            {
                try
                {
                    Windows.Storage.StorageFile audioFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(file));
                    Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                    await audioFile.CopyAsync(localFolder);
                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.Message).ShowAsync();
                }
            }
            //await new MessageDialog("copied").ShowAsync();
        }
    }
}
