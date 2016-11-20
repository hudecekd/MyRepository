using AlarmLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Devices.Sensors;
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
        SimpleOrientationSensor _sensor;

        public AlarmMainPage()
        {
            this.InitializeComponent();

            //if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            //{
            //    return;
            //}

            // initialize VM
            AlarmSettingsVM.Instance.Alarms.Clear(); // occurs whenever we naviage to this page so we need to clear singleton
            foreach (var setting in BaseAlarmSettings.Instance.Alarms)
            {
                var alarmVM = new AlarmSettingVM();
                alarmVM.Initialize(setting);
                AlarmSettingsVM.Instance.Alarms.Add(alarmVM);
            }

            _sensor = SimpleOrientationSensor.GetDefault();
            if (_sensor != null) // for example PC does not have to have sensor
            {
                _sensor.OrientationChanged += Sensor_OrientationChanged;
            }
        }

        private void Sensor_OrientationChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DataTemplate template;
                if ((args.Orientation == SimpleOrientation.Rotated90DegreesCounterclockwise) ||
                    (args.Orientation == SimpleOrientation.Rotated270DegreesCounterclockwise))
                {
                    template = resourceLandscapeTemplate;
                }
                else // otherwise use default template which is "portrait"
                {
                    template = resourcePortraitTemplate;
                }

                lvAlarms.ItemTemplate = template;
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            lvAlarms.DataContext = AlarmSettingsVM.Instance.Alarms;
        }

        // TODO: maybe there is better place to "update" page?
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // this page is the first one => disable back button
            //Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;

            base.OnNavigatedTo(e);
        }

        private async void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // use only hours, minutes and seconds
                var time = DateTime.Now.TimeOfDay;
                time = time.Subtract(TimeSpan.FromTicks(time.Ticks % TimeSpan.TicksPerMinute)); // remove seconds, milliseconds

                Frame.Navigate(typeof(AlarmSettingPage), new AlarmSettingVM()
                {
                    Id = AlarmSettingsVM.Instance.GetNewId(),
                    State = AlarmSettingState.New,
                    Time = time, // select current time by default
                    DateTimeOffset = DateTimeOffset.Now,
                    ImageFilename = "alarm.png",
                    IgnoreHolidays = true // by default we will ignore holidays
                });
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.ToString()).ShowAsync();
            }
        }

        private void lvAlarms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // can occur if alarm is deleted for example.
            if (lvAlarms.SelectedItem == null)
                return;

            var alarm = lvAlarms.SelectedItem as AlarmSettingVM;
            alarm.State = AlarmSettingState.Edit;

            Frame.Navigate(typeof(AlarmSettingPage), alarm);
        }

        private async void btnShowHolidays_Click(object sender, RoutedEventArgs e)
        {
            var holidaysDialog = new HolidaysDialog();
            await holidaysDialog.ShowAsync();
        }
    }
}
