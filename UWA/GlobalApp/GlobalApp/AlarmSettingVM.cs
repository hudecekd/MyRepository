using AlarmLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Popups;

namespace GlobalApp
{
    public enum AlarmSettingState
    {
        New,
        Edit
    }

    public class AlarmSettingVM : INotifyPropertyChanged
    {
        #region Properties from BaseAlarmSetting
        /// <summary>
        /// Id used to identify alarm.
        /// Used by toast so they can be easily removed and replaced without affecting other alarm toasts.
        /// </summary>
        public int Id { get; set; }

        private TimeSpan _time;
        public TimeSpan Time
        {
            get { return _time; }
            set { _time = value; OnPropertyChanged(); }
        }

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

        public bool UseMonday { get; set; }
        public bool UseTuesday { get; set; }
        public bool UseThursday { get; set; }
        public bool UseWednesday { get; set; }
        public bool UseFriday { get; set; }
        public bool UseSaturday { get; set; }
        public bool UseSunday { get; set; }
        #endregion


        public ICommand EnableAlarmCommand { protected set; get; }

        async void ExecuteDeleteCommand(object param)
        {
            try
            {
                var setting = BaseAlarmSettings.Instance.Alarms.Single(s => s.Id == this.Id);
                AlarmManager.Instance.DisableAlarm(setting);
                if (Enabled) AlarmManager.Instance.EnableAlarm(setting); // anable it when it should be (toasts will be updated = created again)

                // synchronize with model
                setting.Enabled = Enabled;

                // check all alarms and if there is at least one enabled
                // then show badge
                var numberOfActiveAlarms = BaseAlarmSettings.Instance.Alarms.Count(a => a.Enabled);
                if (numberOfActiveAlarms > 0)
                {
                    BadgeManager.ShowBadge((uint)numberOfActiveAlarms);
                }
                else
                {
                    BadgeManager.HideBadge();
                }
            }
            catch (Exception ex)
            {
                string message = $"There was a problem enabling/disabling alarm.{Environment.NewLine}Error: {ex.Message}";
                await new MessageDialog(message).ShowAsync();
            }
        }

        public AlarmSettingVM()
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

        public bool OnlyOnce
        {
            get { return Occurrence == OccurrenceType.OnlyOnce; }
            set { Occurrence = (value ? OccurrenceType.OnlyOnce : OccurrenceType.Repeatedly); }
        }

        public bool Repeatedly
        {
            get { return Occurrence == OccurrenceType.Repeatedly; }
            set { Occurrence = (value ? OccurrenceType.Repeatedly : OccurrenceType.OnlyOnce); }
        }

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool CheckAlarmDateTime(DateTime dateTime)
        {
            // do not schedule notification for date & time which has already passed
            // WARNING: But there is still very small possibility that this can occur and
            // we won't catch that.
            return (dateTime >= DateTime.Now);
        }

        /// <summary>
        /// Initializes VM from model.
        /// </summary>
        /// <param name="setting"></param>
        public void Initialize(BaseAlarmSetting setting)
        {
            this.Id = setting.Id;
            this.Enabled = setting.Enabled;
            this.Time = setting.Time;
            this.DateTimeOffset = setting.DateTimeOffset;
            this.AudioFilename = setting.AudioFilename;
            this.Occurrence = setting.Occurrence;

            this.UseMonday = setting.UseMonday;
            this.UseTuesday = setting.UseTuesday;
            this.UseWednesday = setting.UseWednesday;
            this.UseThursday = setting.UseThursday;
            this.UseFriday = setting.UseFriday;
            this.UseSaturday = setting.UseSaturday;
            this.UseSunday = setting.UseSunday;
        }
    }
}
