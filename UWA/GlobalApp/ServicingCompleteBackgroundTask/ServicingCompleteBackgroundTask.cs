using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace ServicingCompleteBackgroundTask
{
    public sealed class ServicingCompleteBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            AppVersion.UpdateSettings();

            AlarmLibrary.BaseAlarmSettings.Instance.ServicingCompleteLastRun = DateTimeOffset.Now;
        }
    }
}
