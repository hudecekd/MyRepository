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
        public AlarmSettingPage()
        {
            this.InitializeComponent();

            InitializeAudioOptions();
            InitializeImageOptions();
        }

        private void InitializeAudioOptions()
        {
            cmbSounds.Items.Clear();
            foreach (var audioFileName in AlarmManager.Instance.GetAudioFiles())
            {
                cmbSounds.Items.Add(audioFileName);
            }
        }

        private void InitializeImageOptions()
        {
            cmbImages.Items.Clear();
            foreach (var audioFileName in AlarmManager.Instance.GetImageiles())
            {
                cmbImages.Items.Add(audioFileName);
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            #region Validation
            var settingVM = DataContext as AlarmSettingVM;
            if (string.IsNullOrWhiteSpace( settingVM.AudioFilename))
            {
                await new MessageDialog("You have to select audio file.").ShowAsync();
                return;
            }

            // no day selected
            if (settingVM.Repeatedly &&
            (!chbMonday.IsChecked.Value) && (!chbTuesday.IsChecked.Value) && (!chbWednesday.IsChecked.Value) &&
            (!chbThursday.IsChecked.Value) && (!chbFriday.IsChecked.Value) && (!chbSaturday.IsChecked.Value) &&
            (!chbSunday.IsChecked.Value))
            {
                await new MessageDialog("You have to select at least one day").ShowAsync();
                return;
            }
            #endregion  

            // synchronize VM to Model
            var setting = (settingVM.State == AlarmSettingState.New ? new BaseAlarmSetting() : BaseAlarmSettings.Instance.Alarms.Single(s => s.Id == settingVM.Id));

            setting.Time = settingVM.Time;
            setting.Enabled = settingVM.Enabled;
            setting.AudioFilename = settingVM.AudioFilename;
            setting.ImageFilename = settingVM.ImageFilename;

            if (rbOnlyOnce.IsChecked.Value) setting.Occurrence = OccurrenceType.OnlyOnce;
            if (rbRepeatedly.IsChecked.Value) setting.Occurrence = OccurrenceType.Repeatedly;

            setting.DateTimeOffset = settingVM.DateTimeOffset;

            setting.DaysOfWeek = DayOfWeekType.None;
            if (chbMonday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Monday;
            if (chbTuesday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Tuesday;
            if (chbWednesday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Wednesday;
            if (chbThursday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Thursday;
            if (chbFriday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Friday;
            if (chbSaturday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Saturday;
            if (chbSunday.IsChecked.Value) setting.DaysOfWeek = setting.DaysOfWeek | DayOfWeekType.Sunday;

            if (settingVM.State == AlarmSettingState.New)
            {
                setting.Id = settingVM.Id;
                BaseAlarmSettings.Instance.Alarms.Add(setting);

                var alarmVM = new AlarmSettingVM();
                alarmVM.Initialize(setting);
                AlarmSettingsVM.Instance.Alarms.Add(alarmVM); // add VM for alarm
            }
            else
            {
                var alarmVM = AlarmSettingsVM.Instance.Alarms.Single(a => a.Id == setting.Id);
                alarmVM.Initialize(setting);
            }

            Frame.GoBack();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var alarm = e.Parameter as AlarmSettingVM;
            DataContext = alarm;

            // set UI controls which are not bound
            if (alarm.Occurrence == OccurrenceType.OnlyOnce) rbOnlyOnce.IsChecked = true;
            if (alarm.Occurrence == OccurrenceType.Repeatedly) rbRepeatedly.IsChecked = true;

            //cmbSounds.SelectedItem = alarm.AudioFilename;

            // enable back button
            //Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;

            base.OnNavigatedTo(e);
        }   

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // update model
            // for update of VM is responsible following page
            var alarm = DataContext as AlarmSettingVM;
            var setting = BaseAlarmSettings.Instance.Alarms.Single(s => s.Id == alarm.Id);
            BaseAlarmSettings.Instance.Alarms.Remove(setting);

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

        private async void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                CopyImageToLocalImageFolder(file);

                var alarm = DataContext as AlarmSettingVM;
                alarm.ImageFilename = file.Name;
            }
            else
            {
                var alarm = DataContext as AlarmSettingVM;
                alarm.ImageFilename = "alarm.png";
            }
        }

        public void CopyImageToLocalImageFolder(Windows.Storage.StorageFile srtFile)
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var targetFolder = default(Windows.Storage.StorageFolder);
            var targetItem = localFolder.TryGetItemAsync(AlarmManager.ImagesFolderName).AsTask().GetAwaiter().GetResult();
            // folder (or file!!!) does not exist
            if (targetItem == null)
            {
                targetFolder = localFolder.CreateFolderAsync(AlarmManager.ImagesFolderName).AsTask().GetAwaiter().GetResult();
            }
            else
            {
                targetFolder = targetItem as Windows.Storage.StorageFolder;
            }

            // copy files from package folder to target folder so we are able to play them in toasts.
            var installationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            
                srtFile.CopyAsync(targetFolder, srtFile.Name, Windows.Storage.NameCollisionOption.ReplaceExisting)
                    .AsTask().GetAwaiter().GetResult();
        }
    }
}
