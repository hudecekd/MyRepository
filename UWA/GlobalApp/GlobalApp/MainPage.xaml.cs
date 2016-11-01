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
    }
}
