using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    }
}
