using AlarmLibrary;
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
using Windows.Networking.Connectivity;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GlobalApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // task so it won't hang up (because we wait for the Dispatcher.RunAsync!)
            // and it would hang up on UI thread.
            Task.Factory.StartNew(() => { UpdateConnectionStatus(); });
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += AlarmSettingPage_BackRequested;
        }


        private void AlarmSettingPage_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            // for now we expect that pivot contains
            // - Pivot items
            // - items contain only grid
            // - grid contains only one item
            var pivotItem = mainPivot.SelectedItem as PivotItem;
            if (pivotItem != null)
            {
                var grid = pivotItem.Content as Grid;
                if (grid != null && grid.Children.Count == 1)
                {
                    var backNavigation = grid.Children.Single() as IBackButtonNavigation;
                    if (backNavigation != null)
                    {
                        e.Handled = backNavigation.NavigateBack();
                        if (e.Handled) return;
                    }
                }
            }

            var frame = Window.Current.Content as Frame;
            if (frame.CanGoBack)
            {
                frame.GoBack();
                e.Handled = true;
            }
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            UpdateConnectionStatus();

            GetHolidays();
        }

        private void UpdateConnectionStatus()
        { 
            var text = string.Empty;
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile == null)
            {
                text = "Unknown";
            }
            else
            {
                var level = connectionProfile.GetNetworkConnectivityLevel();
                switch (level)
                {
                    case NetworkConnectivityLevel.ConstrainedInternetAccess:
                        text = "Constrained Internet Access";
                        break;
                    case NetworkConnectivityLevel.InternetAccess:
                        text = "Internet Access";
                        break;
                    case NetworkConnectivityLevel.LocalAccess:
                        text = "Local Access";
                        break;
                    case NetworkConnectivityLevel.None:
                        text = "None";
                        break;
                }
            }

            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                tbConnectionStatus.Text = text;
            }).AsTask().GetAwaiter().GetResult();
        }

        public void OpenAlarmSetting(int alarmId)
        {
            this.mainPivot.SelectedItem = this.piAlarm;

            // WARNING
            // MV initialization is done when page is loaded
            // so we cannot be sure that it is done when we 
            // set the page this way.
            // We should initialize all MV immediatelly and not when page
            // is loaded???

            var setting = BaseAlarmSettings.Instance.Alarms.Single(s => s.Id == alarmId);

            var alarm = new AlarmSettingVM();
            alarm.Initialize(setting);
            alarm.State = AlarmSettingState.Edit;

            Frame.Navigate(typeof(AlarmSettingPage), alarm);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GetHolidays();
        }

        /// <summary>
        /// For vary rar possibility that multiple calls would occur simultaneously.
        /// TODO: Who to do not wait if it is already running and just ignore second call?
        /// </summary>
        private object _getHolidaysLock = new object();

        private void GetHolidays()
        {
            try
            {
                lock (_getHolidaysLock)
                {
                    var internetAvailable = false;
                    var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
                    if (connectionProfile != null)
                    {
                        if (connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess)
                        {
                            internetAvailable = true;
                        }
                    }
                    if (!internetAvailable) return; // TODO: when internet available again then try to load holidays

                    Task.Factory.StartNew(() =>
                    {
                        // TODO: handle change from one year to another. We need to load its holidays too!!!
                        var holidays = HolidaysApi.GetHolidays(2016, CountryCode.Cze);
                        BaseAlarmSettings.Instance.UpdateHolidays(holidays);
                    });
                }
            }
            catch (Exception ex) // whatever happens just ignore it for now and keep previous holidays.
            {
            }
        }
    }
}
