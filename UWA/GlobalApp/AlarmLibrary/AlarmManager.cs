using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace AlarmLibrary
{
    public class AlarmManager
    {

        private bool CheckAlarmDateTime(DateTime dateTime)
        {
            // do not schedule notification for date & time which has already passed
            // WARNING: But there is still very small possibility that this can occur and
            // we won't catch that.
            return (dateTime >= DateTime.Now);
        }

        public void EnableAlarm(BaseAlarmSetting alarm)
        {
            try
            {
                if (alarm.Occurrence == OccurrenceType.OnlyOnce)
                {
                    var dateTime = alarm.DateTime.Add(alarm.Time);
                    if (!CheckAlarmDateTime(dateTime)) return;

                    CreateNotification(alarm.Id, alarm.AudioFilename, dateTime.ToUniversalTime());
                }
                else // repeatedly is checked
                {
                    // TODO: use only day properties not aggregated property!

                    PlanFutureAlarms(alarm);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void PlanFutureAlarms(BaseAlarmSetting alarm)
        {
            // get already planned toasts for this alarm
            // so we can won't plan it more then once.
            var plannedToasts = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications()
                .Where(t => t.Id == alarm.Id.ToString()).ToList();

            var days = new List<DayOfWeek>();
            if (alarm.UseMonday) days.Add(DayOfWeek.Monday);
            if (alarm.UseTuesday) days.Add(DayOfWeek.Tuesday);
            if (alarm.UseThursday) days.Add(DayOfWeek.Thursday);
            if (alarm.UseWednesday) days.Add(DayOfWeek.Wednesday);
            if (alarm.UseFriday) days.Add(DayOfWeek.Friday);
            if (alarm.UseSaturday) days.Add(DayOfWeek.Saturday);
            if (alarm.UseSunday) days.Add(DayOfWeek.Sunday);

            // plan 14 days ahead
            // TODO: Background task has to be used to plan future toasts.
            var date = DateTime.Now.Date;
            for (int i = 0; i < 14; i++)
            {
                var plannedDate = date.AddDays(i);
                if (days.Contains(plannedDate.DayOfWeek)) // day is checked to be used for alarm
                {
                    var dateTime = plannedDate.Add(alarm.Time);
                    if (!CheckAlarmDateTime(dateTime)) continue;

                    // this alarm is already planned
                    // skip planning
                    if (plannedToasts.Any(t => t.DeliveryTime == dateTime))
                        continue;

                    CreateNotification(alarm.Id, alarm.AudioFilename, dateTime.ToUniversalTime());
                }
            }
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

        public void DisableAlarm(BaseAlarmSetting alarm)
        {
            ClearAlarmsNotifications(alarm.Id);
        }

        /// <summary>
        /// Clears all toast nofitications.
        /// For now it removes notifications for all alarms not just the one being modified!
        /// </summary>
        private void ClearAlarmsNotifications(int alarmId)
        {
            var updater = ToastNotificationManager.CreateToastNotifier();
            var scheduledNotifications = updater.GetScheduledToastNotifications();
            foreach (var scheduledNotification in scheduledNotifications)
            {
                // check that toast is associated with this alarm
                if (scheduledNotification.Id == alarmId.ToString())
                {
                    updater.RemoveFromSchedule(scheduledNotification);
                }
            }
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
