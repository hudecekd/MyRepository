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

        private object _holidaysLock = new object();
        private List<Holiday> Holidays { get; set; } = new List<Holiday>();

        private BaseAlarmSettings()
        {
        }

        /// <summary>
        /// Format used to properly serialize <see cref="DateTimeOffset"/> to JSON and back.
        /// </summary>
        private const string DateTimeOffsetSerializationFormat = "yyyy-MM-ddTHH:mm:ss.fffffffzzz";

        private const string AlarmsKey = "Alarms";

        private const string JsonId = "id";
        private const string JsonEnabled = "enabled";
        private const string JsonTime = "time";
        private const string JsonDaysOfWeek = "daysOfWeek";
        private const string JsonAudioFilename = "audioFilename";
        private const string JsonImageFilename = "imageFilename";
        private const string JsonOccurrence = "occurrence";
        private const string JsonIgnoreHolidays = "ignoreHolidays";
        private const string JsonDateTimeOffset = "dateTimeOffset";

        #region Holiday
        private const string JsonHolidaysKey = "holidays";

        private const string JsonHolidayDate = "date";
        private const string JsonHolidayLocalDescription = "localDescription";
        private const string JsonHolidayEnglishDescription = "englishDescription";
        #endregion

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
                alarmJson[JsonIgnoreHolidays] = JsonValue.CreateBooleanValue(alarm.IgnoreHolidays);
                alarmJson[JsonDateTimeOffset] = JsonValue.CreateStringValue(alarm.DateTimeOffset.ToString(System.Globalization.CultureInfo.InvariantCulture));

                alarmsJson.Add(alarmJson);
            }

            var holidaysJson = new Windows.Data.Json.JsonArray();
            foreach (var holiday in Holidays)
            {
                var holidayJson = new Windows.Data.Json.JsonObject();
                holidayJson[JsonHolidayDate] = JsonValue.CreateNumberValue(holiday.Date.Ticks);
                holidayJson[JsonHolidayLocalDescription] = JsonValue.CreateStringValue(holiday.LocalDescription);
                holidayJson[JsonHolidayEnglishDescription] = JsonValue.CreateStringValue(holiday.EnglishDescription);

                holidaysJson.Add(holidayJson);
            }

            // WARNING: maximum size can be 8KB!!!
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[AlarmsKey] = alarmsJson.Stringify();
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[JsonHolidaysKey] = holidaysJson.Stringify();
        }

        public void LoadSettings()
        {
            LoadAlarmSettingsVersion1();
            LoadHolidaysSettingsVersion1();
        }

        public DateTimeOffset? ServicingCompleteLastRun
        {
            get
            {
                if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("ServicingCompleteLastRun") == false)
                    return null;
                var dateTime = Windows.Storage.ApplicationData.Current.LocalSettings.Values["ServicingCompleteLastRun"] as DateTimeOffset?;
                return dateTime;
            }
            set
            {
                if (value == null) Windows.Storage.ApplicationData.Current.LocalSettings.Values.Remove("ServicingCompleteLastRun");
                if (value != null) Windows.Storage.ApplicationData.Current.LocalSettings.Values["ServicingCompleteLastRun"] = value.Value;
            }
        }

        /// <summary>
        /// Used to clear all settings.
        /// For example if error occurs during loading of setting.
        /// </summary>
        public void Clear()
        {
            Windows.Storage.ApplicationData.Current.LocalSettings.Values.Clear();
        }

        public void ConvertVersion0To1()
        {
            LoadAlarmSettingsVersion0();

            // make modifications which are equal to default setting for new version. In this case version 1 by default ignores holidays.
            foreach (var alarm in Alarms) alarm.IgnoreHolidays = true;

            SaveSettings(); // use default save settings method. Settings should be already converted to the lastest version before saving.
        }

        private void LoadAlarmSettingsVersion0()
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

                // Fix: serialization of DateTimeOffset was missing. I do not want to change "date version" so I just check whether key is available here.
                if (alarmJson.ContainsKey(JsonDateTimeOffset))
                    alarm.DateTimeOffset = DateTimeOffset.Parse(alarmJson[JsonDateTimeOffset].GetString(), System.Globalization.CultureInfo.InvariantCulture);

                Alarms.Add(alarm);
            }
        }

        private void LoadAlarmSettingsVersion1()
        {
            LoadAlarmSettingsVersion0();

            // WARNING: what if settings is somehow changed/removed between loading of previous version and
            // loading of modification for the latest version below?

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
                var alarmId = int.Parse(alarmJson[JsonId].GetString());
                var alarmIgnoreHolidays = alarmJson[JsonIgnoreHolidays].GetBoolean();
                var alarm = Alarms.Single(a => a.Id == alarmId);

                // modification made to the version 1.
                alarm.IgnoreHolidays = alarmIgnoreHolidays;
            }
        }

        private void LoadHolidaysSettingsVersion1()
        {
            var newHolidays = new List<Holiday>();

            // same as for alarm (versioning problem)
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(JsonHolidaysKey) == false)
                return;

            var settingsStr = Windows.Storage.ApplicationData.Current.LocalSettings.Values[JsonHolidaysKey] as string;
            var holidaysJson = JsonValue.Parse(settingsStr).GetArray();

            foreach (var holidayJsonValue in holidaysJson)
            {
                var holidayJson = holidayJsonValue.GetObject();
                var holiday = new Holiday();
                holiday.Date = new DateTime((long)holidayJson[JsonHolidayDate].GetNumber());
                holiday.LocalDescription = holidayJson[JsonHolidayLocalDescription].GetString();
                holiday.EnglishDescription = holidayJson[JsonHolidayEnglishDescription].GetString();

                newHolidays.Add(holiday);
            }

            UpdateHolidays(newHolidays);
        }

        public void UpdateHolidays(IEnumerable<Holiday> newHolidays)
        {
            lock (_holidaysLock)
            {
                Holidays.Clear();
                Holidays.AddRange(newHolidays);
            }
        }

        public Holiday GetHoliday(DateTime dateTime)
        {
            lock (_holidaysLock)
            {
                return Holidays.SingleOrDefault(h => h.Date.Date == dateTime.Date);
            }
        }

        /// <summary>
        /// Returns copied collection of holidays.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Holiday> GetHolidays()
        {
            lock (_holidaysLock)
            {
                return Holidays.ToArray();
            }
        }
    }
}
