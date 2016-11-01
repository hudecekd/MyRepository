using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmLibrary
{
    public class BaseAlarmSetting
    {
        /// <summary>
        /// Id used to identify alarm.
        /// Used by toast so they can be easily removed and replaced without affecting other alarm toasts.
        /// </summary>
        public int Id { get; set; }
        public TimeSpan Time { get; set; }
        public bool Enabled { get; set; }

        /// <summary>
        /// Name of the file to be played.
        /// We do not use some unique "id" now.
        /// </summary>
        public string AudioFilename { get; set; }

        public OccurrenceType Occurrence { get; set; }

        /// <summary>
        /// Used only when <see cref="Occurrence"/> is equel to <see cref="OccurrenceType.OnlyOnce"/>.
        /// </summary>
        public DateTimeOffset DateTimeOffset { get; set; }

        #region Days of week
        public DayOfWeekType DaysOfWeek { get; set; }

        protected bool GetDayBoolean(DayOfWeekType dayOfWeek)
        {
            return (DaysOfWeek & dayOfWeek) == dayOfWeek;
        }

        protected void SetDayBoolean(DayOfWeekType dayOfWeek, bool use)
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
        #endregion
    }
}
