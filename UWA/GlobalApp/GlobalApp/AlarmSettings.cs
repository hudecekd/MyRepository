using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

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

        public void SaveSettings()
        {
            var alarmsJson = new Windows.Data.Json.JsonArray();
            foreach (var alarm in Alarms)
            {
                var alarmJson = new Windows.Data.Json.JsonObject();
                alarmJson["enabled"] = JsonValue.CreateStringValue(alarm.Enabled.ToString());
                alarmJson["time"] = JsonValue.CreateStringValue(alarm.Time.Ticks.ToString());
                alarmJson["daysOfWeek"] = JsonValue.CreateStringValue(((int)alarm.DaysOfWeek).ToString());

                alarmsJson.Add(alarmJson);
            }

            // WARNING: maximum size can be 8KB!!!
            Windows.Storage.ApplicationData.Current.LocalSettings.Values["Alarms"] = alarmsJson.Stringify();
        }

        public void LoadSettings()
        {
            var settingsStr = Windows.Storage.ApplicationData.Current.LocalSettings.Values["Alarms"] as string;
            var alarmsJson = JsonValue.Parse(settingsStr).GetArray();

            Alarms.Clear();
            foreach (var alarmJsonValue in alarmsJson)
            {
                var alarmJson = alarmJsonValue.GetObject();
                var alarm = new AlarmSetting();
                alarm.Enabled = Boolean.Parse(alarmJson["enabled"].GetString());
                alarm.Time = new TimeSpan(long.Parse(alarmJson["time"].GetString()));
                alarm.DaysOfWeek = (DayOfWeekType)int.Parse(alarmJson["daysOfWeek"].GetString());

                Alarms.Add(alarm);
            }
        }
    }

    public enum DayOfWeekType
    {
        Monday = 1,
        Tuesday = 1 << 2,
        Wednesday = 1 << 4,
        Thursday = 1 << 5,
        Friday = 1 << 6,
        Saturday = 1 << 7,
        Sunday = 1 << 8
    }

    public enum AlarmSettingState
    {
        New,
        Edit
    }

    public class AlarmSetting : INotifyPropertyChanged
    {
        public TimeSpan Time { get; set; }
        public DayOfWeekType DaysOfWeek { get; set; }

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

        private bool GetDayBoolean(DayOfWeekType dayOfWeek)
        {
            return (DaysOfWeek & dayOfWeek) == dayOfWeek;
        }

        private void SetDayBoolean(DayOfWeekType dayOfWeek, bool use)
        {
            if (use) DaysOfWeek |= dayOfWeek; else DaysOfWeek &= ~dayOfWeek;
        }

        public bool UseMonday
        {
            get { return GetDayBoolean(DayOfWeekType.Monday); }
            set { SetDayBoolean(DayOfWeekType.Monday, value); }
        }
        public bool UseTuesday
        {
            get { return GetDayBoolean(DayOfWeekType.Tuesday); }
            set { SetDayBoolean(DayOfWeekType.Tuesday, value); }
        }
        public bool UseThursday
        {
            get { return GetDayBoolean(DayOfWeekType.Thursday); }
            set { SetDayBoolean(DayOfWeekType.Thursday, value); }
        }
        public bool UseWednesday
        {
            get { return GetDayBoolean(DayOfWeekType.Wednesday); }
            set { SetDayBoolean(DayOfWeekType.Wednesday, value); }
        }
        public bool UseFriday
        {
            get { return GetDayBoolean(DayOfWeekType.Friday); }
            set { SetDayBoolean(DayOfWeekType.Friday, value); }
        }
        public bool UseSaturday
        {
            get { return GetDayBoolean(DayOfWeekType.Saturday); }
            set { SetDayBoolean(DayOfWeekType.Saturday, value); }
        }
        public bool UseSunday
        {
            get { return GetDayBoolean(DayOfWeekType.Sunday); }
            set { SetDayBoolean(DayOfWeekType.Sunday, value); }
        }

        private bool _enabled = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;

                // TODO: logic
                // TODO: only if it is changed by use not at initialization!
                // TODO: or do logit at save or end of the application?
            }
        }
    }
}
