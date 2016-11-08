using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace AlarmLibrary
{
    public sealed class BaseAlarmSettings
    {
        private static Lazy<BaseAlarmSettings> _instance { get; } = new Lazy<BaseAlarmSettings>(() => new BaseAlarmSettings());

        /// <summary>
        /// WARNING:
        /// It is used by background task and UWA.
        /// How singleton is handled in this scenario???
        /// </summary>
        public static BaseAlarmSettings Instance { get { return _instance.Value; } }

        public ObservableCollection<BaseAlarmSetting> Alarms { get; private set; } = new ObservableCollection<BaseAlarmSetting>();

        private BaseAlarmSettings()
        {

        }

        private const string AlarmsKey = "Alarms";

        private const string JsonId = "id";
        private const string JsonEnabled = "enabled";
        private const string JsonTime = "time";
        private const string JsonDaysOfWeek = "daysOfWeek";
        private const string JsonAudioFilename = "audioFilename";
        private const string JsonImageFilename = "imageFilename";
        private const string JsonOccurrence = "occurrence";

        public void SaveSettings()
        {
            var alarmsJson = new Windows.Data.Json.JsonArray();
            foreach (var alarm in Alarms)
            {
                var alarmJson = new Windows.Data.Json.JsonObject();
                alarmJson[JsonId] = JsonValue.CreateStringValue(alarm.Id.ToString());
                alarmJson[JsonEnabled] = JsonValue.CreateStringValue(alarm.Enabled.ToString());
                alarmJson[JsonTime] = JsonValue.CreateStringValue(alarm.Time.Ticks.ToString());
                alarmJson[JsonDaysOfWeek] = JsonValue.CreateStringValue(((int)alarm.DaysOfWeek).ToString());
                alarmJson[JsonAudioFilename] = JsonValue.CreateStringValue(alarm.AudioFilename);
                alarmJson[JsonImageFilename] = JsonValue.CreateStringValue(alarm.ImageFilename);
                alarmJson[JsonOccurrence] = JsonValue.CreateStringValue(alarm.Occurrence.ToString());

                alarmsJson.Add(alarmJson);
            }

            // WARNING: maximum size can be 8KB!!!
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[AlarmsKey] = alarmsJson.Stringify();
        }

        public void LoadSettings()
        {
            // TODO: setting versioning
            Alarms.Clear();

            // we are trying to load setting for the first time => there is nothing
            // when versioning will be supported check mechanism should be better
            // or someone delted the settings.dat file for example.
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(AlarmsKey) == false)
                return;

            var settingsStr = Windows.Storage.ApplicationData.Current.LocalSettings.Values[AlarmsKey] as string;
            var alarmsJson = JsonValue.Parse(settingsStr).GetArray();

            foreach (var alarmJsonValue in alarmsJson)
            {
                var alarmJson = alarmJsonValue.GetObject();
                var alarm = new BaseAlarmSetting();
                alarm.Id = int.Parse(alarmJson[JsonId].GetString());
                alarm.Enabled = Boolean.Parse(alarmJson[JsonEnabled].GetString());
                alarm.Time = new TimeSpan(long.Parse(alarmJson[JsonTime].GetString()));
                alarm.DaysOfWeek = (DayOfWeekType)int.Parse(alarmJson[JsonDaysOfWeek].GetString());
                alarm.AudioFilename = alarmJson[JsonAudioFilename].GetString();
                alarm.ImageFilename = alarmJson[JsonImageFilename].GetString();
                alarm.Occurrence = (OccurrenceType)Enum.Parse(typeof(OccurrenceType), alarmJson[JsonOccurrence].GetString());

                Alarms.Add(alarm);
            }
        }
    }
}
