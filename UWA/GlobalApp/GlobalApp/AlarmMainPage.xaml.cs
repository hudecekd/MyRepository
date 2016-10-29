using AlarmLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class AlarmMainPage : Page
    {
        public AlarmMainPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            lvAlarms.DataContext = AlarmSettings.Instance.Alarms;
        }

        // TODO: maybe there is better place to "update" page?
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AlarmSettingPage), new AlarmSetting()
            {
                Id = AlarmSettings.Instance.GetNewId(),
                State = AlarmSettingState.New,
                Time = DateTime.Now.TimeOfDay // select current time by default
            });
        }

        private void lvAlarms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // can occur if alarm is deleted for example.
            if (lvAlarms.SelectedItem == null)
                return;

            var alarm = lvAlarms.SelectedItem as AlarmSetting;
            alarm.State = AlarmSettingState.Edit;

            Frame.Navigate(typeof(AlarmSettingPage), alarm);
        }

        private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            throw new InvalidOperationException("Do not use this code anymore. CheckBox and Command are used now!");

            try
            {
                var enableSwitch = sender as ToggleSwitch;
                var alarm = enableSwitch.DataContext as AlarmSetting; // get alarm setting associated with ListViewItem (parent of switch)

                var manager = new AlarmManager();
                if (alarm.Enabled)
                    manager.EnableAlarm(alarm);
                else
                    manager.DisableAlarm(alarm);
            }
            catch (Exception ex)
            {
                string message = $"There was a problem enabling/disabling alarm.{Environment.NewLine}Error: {ex.Message}.";
                await new MessageDialog(message).ShowAsync();

                
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
