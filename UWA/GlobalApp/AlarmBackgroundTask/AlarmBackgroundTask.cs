using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using AlarmLibrary;

namespace AlarmBackgroundTask
{
    public sealed class AlarmBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BaseAlarmSettings.Instance.LoadSettings();

            foreach (var alarm in BaseAlarmSettings.Instance.Alarms)
            {
                if (!alarm.Enabled) continue; // no need to plan disabled alarms

                // alarm which occurs only once is planned
                // when app is running so we do not need to check it
                if (alarm.Occurrence == OccurrenceType.OnlyOnce) continue;

                AlarmManager.Instance.PlanFutureAlarms(alarm);
            }
        }
    }
}
