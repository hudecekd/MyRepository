using AlarmLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace AlarmToastBackgroundTask
{
    public sealed class AlarmToastBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // TODO: synchronization with app!!!
            // what if app setting changes but is not saved and suddenly toast notification occurs
            // somehow we need to synchronize the data
            BaseAlarmSettings.Instance.LoadSettings();

            var notification  = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;

            // TODO: make class for storing/retreiving arguments used in toasts
            var arguments = notification.Argument.Split(':');
            var type = arguments[0];
            var alarmIdStr = arguments[1];

            if (type == "snooze")
            {
                var alarmId = int.Parse(alarmIdStr);
                var snoozeTimeStr = notification.UserInput["snoozeTimeId"].ToString();
                var snoozeTimeSeconds = int.Parse(snoozeTimeStr);

                var alarm = BaseAlarmSettings.Instance.Alarms.Single(a => a.Id == alarmId);
                var dateTime = DateTimeOffset.Now.DateTime.Add(TimeSpan.FromMinutes(snoozeTimeSeconds));
                //if (!CheckAlarmDateTime(dateTime)) return;

                //AlarmManager.Instance.CreateNotification(alarm.Id, alarm.AudioFilename, alarm.ImageFilename, dateTime.ToUniversalTime());
            }
            if (type == "dismiss")
            {
                var alarmId = int.Parse(alarmIdStr);;

                var alarm = BaseAlarmSettings.Instance.Alarms.Single(a => a.Id == alarmId);

                // disable alarm in settings. This is just for setting synchronization.
                if (alarm.Occurrence == OccurrenceType.OnlyOnce)
                {
                    var syncData = new AlarmToastSyncData();
                    syncData.AlarmId = alarmId;
                    syncData.Type = AlarmToastSyncData.ToastType.Dismiss;

                    using (var mutex = new MutexWrapper(TimeSpan.FromSeconds(5), TestApplicationInfo.ApplicationId, AlarmToastSyncData.SyncName))
                    {
                        Windows.Storage.ApplicationData.Current.LocalSettings.Values[AlarmToastSyncData.SyncKey] = syncData.Serialize();
                    }

                    alarm.Enabled = false;
                    BaseAlarmSettings.Instance.SaveSettings();
                }
            }
        }
    }
}
