using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace AlarmLibrary
{
    public class BaseAlarmSettings
    {
        public ObservableCollection<BaseAlarmSetting> Alarms { get; private set; } = new ObservableCollection<BaseAlarmSetting>();

        public void SaveSettings()
        {
            var alarmsJson = new Windows.Data.Json.JsonArray();
            foreach (var alarm in Alarms)
            {
                var alarmJson = new Windows.Data.Json.JsonObject();
                alarmJson["id"] = JsonValue.CreateStringValue(alarm.Id.ToString());
                alarmJson["enabled"] = JsonValue.CreateStringValue(alarm.Enabled.ToString());
                alarmJson["time"] = JsonValue.CreateStringValue(alarm.Time.Ticks.ToString());
                alarmJson["daysOfWeek"] = JsonValue.CreateStringValue(((int)alarm.DaysOfWeek).ToString());
                alarmJson["audioFilename"] = JsonValue.CreateStringValue(alarm.AudioFilename);
                alarmJson["occurrence"] = JsonValue.CreateStringValue(alarm.Occurrence.ToString());

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
                var alarm = new BaseAlarmSetting();
                alarm.Id = int.Parse(alarmJson["id"].GetString());
                alarm.Enabled = Boolean.Parse(alarmJson["enabled"].GetString());
                alarm.Time = new TimeSpan(long.Parse(alarmJson["time"].GetString()));
                alarm.DaysOfWeek = (DayOfWeekType)int.Parse(alarmJson["daysOfWeek"].GetString());
                alarm.AudioFilename = alarmJson["audioFilename"].GetString();
                alarm.Occurrence = (OccurrenceType)Enum.Parse(typeof(OccurrenceType), alarmJson["occurrence"].GetString());

                Alarms.Add(alarm);
            }
        }
    }
}
