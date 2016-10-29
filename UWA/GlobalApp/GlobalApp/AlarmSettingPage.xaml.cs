using AlarmLibrary;
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
        private const string AudioFolderName = "Audio";

        public AlarmSettingPage()
        {
            this.InitializeComponent();

            InitializeAudioOptions();
        }

        private void InitializeAudioOptions()
        {
            cmbSounds.Items.Clear();

            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var audioFolder = localFolder.GetFolderAsync(AudioFolderName).AsTask().GetAwaiter().GetResult();
            foreach (var audioFile in audioFolder.GetFilesAsync().AsTask().GetAwaiter().GetResult())
            {
                cmbSounds.Items.Add(audioFile.Name);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // validate
            // add to manager
            // navigate back

            var setting = DataContext as AlarmSetting;
            setting.Time = tpTime.Time;

            setting.DaysOfWeek = DayOfWeekType.None;
            if (chbMonday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Monday;
            if (chbTuesday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Tuesday;
            if (chbWednesday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Thursday;
            if (chbThursday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Wednesday;
            if (chbFriday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Friday;
            if (chbSaturday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Saturday;
            if (chbSunday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Sunday;

            if (rbOnlyOnce.IsChecked.Value) setting.Occurrence = OccurrenceType.OnlyOnce;
            if (rbRepeatedly.IsChecked.Value) setting.Occurrence = OccurrenceType.Repeatedly;

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

        //private void CreateNotification()
        //{
        //    var updater = TileUpdateManager.CreateTileUpdaterForApplication();

        //    var file = cmbSounds.SelectedItem as string;
        //    var fileUriStr = $"ms-appdata:///local/{AudioFolderName}/{file}"; // file is copied to local folder so we use different prefix
        //    var durationStr = (chbDuration.IsChecked.HasValue && chbDuration.IsChecked.Value) ? "long" : "short";
        //    var loopStr = (chbLoop.IsChecked.HasValue && chbLoop.IsChecked.Value ? "true" : "false");
        //    var xmlString = string.Format(toastXml, fileUriStr, loopStr, durationStr);

        //    var xml = new XmlDocument();
        //    xml.LoadXml(xmlString);

        //    var dateTime = dpDate.Date.Date.Add(tpTime.Time).ToUniversalTime();

        //    //var toast = new ScheduledToastNotification(xml, new DateTimeOffset(DateTime.Now.AddSeconds(10).ToUniversalTime()));
        //    var toast = new ScheduledToastNotification(xml, dateTime);

        //    ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
        //}

        

        /// <summary>
        /// Clears all toast nofitications.
        /// For now it removes notifications for all alarms not just the one being modified!
        /// </summary>
        private void ClearAlarmsNotifications()
        {
            var updater = ToastNotificationManager.CreateToastNotifier();
            var scheduledNotifications = updater.GetScheduledToastNotifications();
            foreach (var scheduledNotification in scheduledNotifications)
            {
                updater.RemoveFromSchedule(scheduledNotification);
            }
        }

        private void CreateAlarmNotification()
        {
        }

        private async void btnCopyAudio_Click(object sender, RoutedEventArgs e)
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var targetFolder = default(Windows.Storage.StorageFolder);
            var targetItem = localFolder.TryGetItemAsync(AudioFolderName).AsTask().GetAwaiter().GetResult();
            // folder (or file!!!) does not exist
            if (targetItem == null)
            {
                targetFolder = localFolder.CreateFolderAsync(AudioFolderName).AsTask().GetAwaiter().GetResult();
            }
            else
            {
                targetFolder = targetItem as Windows.Storage.StorageFolder;
            }

            // copy files from package folder to target folder so we are able to play them in toasts.
            var installationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var srcFolder = installationFolder.GetFolderAsync(AudioFolderName).AsTask().GetAwaiter().GetResult();
            var files = srcFolder.GetFilesAsync().AsTask().GetAwaiter().GetResult();
            foreach (var file in files)
            {
                await file.CopyAsync(targetFolder, file.Name, Windows.Storage.NameCollisionOption.ReplaceExisting);
            }
        }

        private void btnCreateNotification_Click(object sender, RoutedEventArgs e)
        {
            ClearAlarmsNotifications();
            CreateAlarmNotification();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var alarm = e.Parameter as AlarmSetting;
            DataContext = alarm;

            // set UI controls which are not bound
            if (alarm.Occurrence == OccurrenceType.OnlyOnce) rbOnlyOnce.IsChecked = true;
            if (alarm.Occurrence == OccurrenceType.Repeatedly) rbRepeatedly.IsChecked = true;

            //cmbSounds.SelectedItem = alarm.AudioFilename;

            base.OnNavigatedTo(e);
        }   

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var alarm = DataContext as AlarmSetting;
            AlarmSettings.Instance.Alarms.Remove(alarm);

            Frame.GoBack();
        }

        private void rbOnlyOnce_Checked(object sender, RoutedEventArgs e)
        {
            spOnlyOnce.Visibility = Visibility.Visible;
            spRepeatedly.Visibility = Visibility.Collapsed;
        }

        private void rbRepeatedly_Checked(object sender, RoutedEventArgs e)
        {
            spOnlyOnce.Visibility = Visibility.Collapsed;
            spRepeatedly.Visibility = Visibility.Visible;
        }
    }
}
