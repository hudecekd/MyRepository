using AlarmLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ServicingCompleteBackgroundTask
{
    public static class AppVersion
    {
        /// <summary>
        /// Represents current version of app data.
        /// </summary>
        public static uint Version { get { return 1; } }

        /// <summary>
        /// Loads setting and updates it to the current version if required.
        /// </summary>
        public static void UpdateSettings()
        {
            try
            {
                if (ApplicationData.Current.Version < Version)
                {
                    ApplicationData.Current.SetVersionAsync(Version, AppDataSetVersion)
                        .AsTask().GetAwaiter().GetResult();
                }

                // Load settings for curren version
                // If version was smaller then data should be converted be SetVersionSasync already.
                //BaseAlarmSettings.Instance.LoadSettings();
            }
            catch (Exception ex)
            {
                // clear settings and load empty settings.
                BaseAlarmSettings.Instance.Clear();
                //BaseAlarmSettings.Instance.LoadSettings();
            }
        }

        internal static void AppDataSetVersion(Windows.Storage.SetVersionRequest request)
        {   
            var deferral = request.GetDeferral();
            switch (request.CurrentVersion)
            {
                case 0:
                    BaseAlarmSettings.Instance.ConvertVersion0To1();
                    break;
            }

            deferral.Complete();
        }
    }
}
