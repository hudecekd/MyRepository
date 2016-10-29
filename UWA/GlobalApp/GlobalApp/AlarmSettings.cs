using AlarmLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Popups;

namespace GlobalApp
{
    public class AlarmSettings
    {
        private static AlarmSettings _settings;
        public static AlarmSettings Instance
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new AlarmSettings();
                    // TODO: load
                }

                return _settings;
            }
        }

        public ObservableCollection<AlarmSetting> Alarms { get; private set; } = new ObservableCollection<AlarmSetting>();

        public int GetNewId()
        {
            int id = 1;
            // either fill space (when alarm was deleted and id is available)
            // or get next highest one which is not used.
            while (Alarms.Any(a => a.Id == id)) id++;
            return id;
        }

        public void SaveSettings()
        {
            // TODO: synchronize VM to base alarm settings
            var settings = new BaseAlarmSettings();
            settings.SaveSettings();
        }

        public void LoadSettings()
        {
            var settings = new BaseAlarmSettings();
            settings.LoadSettings();

            Alarms.Clear();
            foreach (var baseAlarm in settings.Alarms)
            {
                var alarm = new AlarmSetting();
                alarm.Id = baseAlarm.Id;
                alarm.Enabled = baseAlarm.Enabled;
                alarm.Time = baseAlarm.Time;
                alarm.DaysOfWeek = baseAlarm.DaysOfWeek;
                alarm.AudioFilename = baseAlarm.AudioFilename;
                alarm.Occurrence = baseAlarm.Occurrence;

                Alarms.Add(alarm);
            }
        }
    }

    public enum AlarmSettingState
    {
        New,
        Edit
    }

    public class AlarmSetting : BaseAlarmSetting, INotifyPropertyChanged
    {
        public ICommand EnableAlarmCommand { protected set; get; }

        async void ExecuteDeleteCommand(object param)
        {
            try
            {
                var manager = new AlarmManager();
                manager.DisableAlarm(this); // TODO: disable only this alarm not all!!!
                if (Enabled) manager.EnableAlarm(this); // anable it when it should be (toasts will be updated = created again)
            }
            catch (Exception ex)
            {
                string message = $"There was a problem enabling/disabling alarm.{Environment.NewLine}Error: {ex.Message}";
                await new MessageDialog(message).ShowAsync();
            }
        }

        public AlarmSetting()
        {
            this.EnableAlarmCommand = new DelegateCommand(ExecuteDeleteCommand);
        }

        public Windows.UI.Xaml.Visibility DeleteButtonVisibility
        {
            get
            {
                if (State == AlarmSettingState.Edit)
                    return Windows.UI.Xaml.Visibility.Visible;
                else
                    return Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// State of Alarm setting.
        /// Should be only in VM.
        /// </summary>
        public AlarmSettingState State { get; set; }

        private bool _enabled = false;

        public event PropertyChangedEventHandler PropertyChanged;

        private bool CheckAlarmDateTime(DateTime dateTime)
        {
            // do not schedule notification for date & time which has already passed
            // WARNING: But there is still very small possibility that this can occur and
            // we won't catch that.
            return (dateTime >= DateTime.Now);
        }

        private void CreateNotification(int alarmId, string audioFile, DateTime dateTime)
        {
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();

            var fileUriStr = $"ms-appdata:///local/{AudioFolderName}/{audioFile}"; // file is copied to local folder so we use different prefix
            var durationStr = (false) ? "long" : "short";
            var loopStr = (false ? "true" : "false");
            var xmlString = string.Format(toastXml, fileUriStr, loopStr, durationStr);

            var xml = new XmlDocument();
            xml.LoadXml(xmlString);

            var toast = new ScheduledToastNotification(xml, dateTime.ToUniversalTime());
            toast.Id = alarmId.ToString(); // not unique. It is enouqh to identify alarm not "toast"

            ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
        }

        private const string AudioFolderName = "Audio";

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
    }
}
