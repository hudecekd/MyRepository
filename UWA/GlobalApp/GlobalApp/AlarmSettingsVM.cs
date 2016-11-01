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
    public class AlarmSettingsVM
    {
        private static AlarmSettingsVM _settings;
        public static AlarmSettingsVM Instance
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new AlarmSettingsVM();
                    // TODO: load
                }

                return _settings;
            }
        }

        public ObservableCollection<AlarmSettingVM> Alarms { get; private set; } = new ObservableCollection<AlarmSettingVM>();

        public int GetNewId()
        {
            int id = 1;
            // either fill space (when alarm was deleted and id is available)
            // or get next highest one which is not used.
            while (Alarms.Any(a => a.Id == id)) id++;
            return id;
        }

        //public void LoadSettings()
        //{
        //    var settings = new BaseAlarmSettings();
        //    settings.LoadSettings();

        //    Alarms.Clear();
        //    foreach (var baseAlarm in settings.Alarms)
        //    {
        //        var alarm = new AlarmSettingVM();
        //        alarm.Id = baseAlarm.Id;
        //        alarm.Enabled = baseAlarm.Enabled;
        //        alarm.Time = baseAlarm.Time;
        //        alarm.DaysOfWeek = baseAlarm.DaysOfWeek;
        //        alarm.AudioFilename = baseAlarm.AudioFilename;
        //        alarm.Occurrence = baseAlarm.Occurrence;

        //        Alarms.Add(alarm);
        //    }
        //}
    }
}
