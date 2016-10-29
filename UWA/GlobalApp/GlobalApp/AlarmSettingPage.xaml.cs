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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GlobalApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlarmSettingPage : Page
    {
        private List<string> _audioFiles = new List<string>()
        {
            "piano.wav",
            "01 - My Silver Lining (4).mp3"
            };

        public AlarmSettingPage()
        {
            this.InitializeComponent();

            foreach (var file in _audioFiles)
            {
                cmbSounds.Items.Add(file);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // validate
            // add to manager
            // navigate back

            var setting = DataContext as AlarmSetting;
            setting.Time = tpTime.Time;

            if (chbMonday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Monday;
            if (chbTuesday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Tuesday;
            if (chbWednesday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Thursday;
            if (chbThursday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Wednesday;
            if (chbFriday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Friday;
            if (chbSaturday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Saturday;
            if (chbSunday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Sunday;


            if (setting.State == AlarmSettingState.New)
                AlarmSettings.Instance.Alarms.Add(setting);
            else
            {
                // TODO: Do i need to do something?
                // It is already added to the collestion so there should be ne need
                // to modify something.

                // TODO: but what about cancel?
                // Need to separate VM from data!!! Otherwise it will remain changed.
            }

            Frame.GoBack();
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

        private void CreateNotification()
        {
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();

            var file = cmbSounds.SelectedItem as string;
            var fileUriStr = "ms-appdata:///" + file; // file is copied to local folder so we use different prefix
            var durationStr = (chbDuration.IsChecked.HasValue && chbDuration.IsChecked.Value) ? "long" : "short";
            var loopStr = (chbLoop.IsChecked.HasValue && chbLoop.IsChecked.Value ? "true" : "false");
            var xmlString = string.Format(toastXml, fileUriStr, loopStr, durationStr);

            var xml = new XmlDocument();
            xml.LoadXml(xmlString);

            var dateTime = dpDate.Date.Date.Add(tpTime.Time).ToUniversalTime();

            //var toast = new ScheduledToastNotification(xml, new DateTimeOffset(DateTime.Now.AddSeconds(10).ToUniversalTime()));
            var toast = new ScheduledToastNotification(xml, dateTime);

            ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
        }

        private async void btnCopyAudio_Click(object sender, RoutedEventArgs e)
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var installationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var assetsFolder = installationFolder.GetFolderAsync("Audio").AsTask().GetAwaiter().GetResult();
            var files = assetsFolder.GetFilesAsync().AsTask().GetAwaiter().GetResult();
            foreach (var file in files)
            {
                await file.CopyAsync(localFolder, file.Name, Windows.Storage.NameCollisionOption.ReplaceExisting);
            }
            return;

            foreach (var file in _audioFiles)
            {
                try
                {
                    var uriFile = new Uri("ms-appx:///Assets/" + file);
                    Windows.Storage.StorageFile audioFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uriFile);
                    await audioFile.CopyAsync(localFolder, audioFile.Name, Windows.Storage.NameCollisionOption.ReplaceExisting);
                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.Message).ShowAsync();
                }
            }
        }

        private void btnCreateNotification_Click(object sender, RoutedEventArgs e)
        {
            CreateNotification();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = e.Parameter;

            base.OnNavigatedTo(e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var alarm = DataContext as AlarmSetting;
            AlarmSettings.Instance.Alarms.Remove(alarm);

            Frame.GoBack();
        }
    }
}
