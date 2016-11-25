using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

using Microsoft.Toolkit.Uwp.Notifications;

namespace AlarmLibrary
{
    /// <summary>
    /// Manages operations with alarm.
    /// </summary>
    public class AlarmManager
    {
        private static Lazy<AlarmManager> _instance { get; } = new Lazy<AlarmManager>(() => new AlarmManager());

        /// <summary>
        /// WARNING:
        /// It is used by background task and UWA.
        /// How singleton is handled in this scenario???
        /// </summary>
        public static AlarmManager Instance { get { return _instance.Value; } }

        private AlarmManager()
        {
        }

        public void CopyAudioFiles()
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var targetFolder = default(Windows.Storage.StorageFolder);
            var targetItem = localFolder.TryGetItemAsync(AudioFolderName).AsTask().GetAwaiter().GetResult();
            // folder (or file!!!) does not exist
            if (targetItem == null)
            {
                targetFolder = localFolder.CreateFolderAsync(AudioFolderName).AsTask().GetAwaiter().GetResult();
            }
            else
            {
                targetFolder = targetItem as Windows.Storage.StorageFolder;
            }

            // copy files from package folder to target folder so we are able to play them in toasts.
            var installationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var srcFolder = installationFolder.GetFolderAsync(AudioFolderName).AsTask().GetAwaiter().GetResult();
            var files = srcFolder.GetFilesAsync().AsTask().GetAwaiter().GetResult();
            foreach (var file in files)
            {
                file.CopyAsync(targetFolder, file.Name, Windows.Storage.NameCollisionOption.ReplaceExisting)
                    .AsTask().GetAwaiter().GetResult();
            }
        }

        public void CopyImages()
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var targetFolder = default(Windows.Storage.StorageFolder);
            var targetItem = localFolder.TryGetItemAsync(ImagesFolderName).AsTask().GetAwaiter().GetResult();
            // folder (or file!!!) does not exist
            if (targetItem == null)
            {
                targetFolder = localFolder.CreateFolderAsync(ImagesFolderName).AsTask().GetAwaiter().GetResult();
            }
            else
            {
                targetFolder = targetItem as Windows.Storage.StorageFolder;
            }

            // copy files from package folder to target folder so we are able to play them in toasts.
            var installationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var srcFolder = installationFolder.GetFolderAsync(ImagesFolderName).AsTask().GetAwaiter().GetResult();
            var files = srcFolder.GetFilesAsync().AsTask().GetAwaiter().GetResult();
            foreach (var file in files)
            {
                file.CopyAsync(targetFolder, file.Name, Windows.Storage.NameCollisionOption.ReplaceExisting)
                    .AsTask().GetAwaiter().GetResult();
            }
        }

        public IEnumerable<string> GetAudioFiles()
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            // WARNING: hot fix. When folder does not exists on mobile yet exception is thrown
            // Folder should be created before it is used!!!
            // So files should be copied there before accessing it.
            var audioFolderItem = localFolder.TryGetItemAsync(AudioFolderName).AsTask().GetAwaiter().GetResult();
            if (audioFolderItem == null) return new string[0];

            var audioFolder = localFolder.GetFolderAsync(AudioFolderName).AsTask().GetAwaiter().GetResult();
            return audioFolder.GetFilesAsync()
                .AsTask().GetAwaiter().GetResult()
                .Select(f => f.Name);
        }

        public IEnumerable<string> GetImageiles()
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            // WARNING: hot fix. When folder does not exists on mobile yet exception is thrown
            // Folder should be created before it is used!!!
            // So files should be copied there before accessing it.
            var audioFolderItem = localFolder.TryGetItemAsync(ImagesFolderName).AsTask().GetAwaiter().GetResult();
            if (audioFolderItem == null) return new string[0];

            var audioFolder = localFolder.GetFolderAsync(ImagesFolderName).AsTask().GetAwaiter().GetResult();
            return audioFolder.GetFilesAsync()
                .AsTask().GetAwaiter().GetResult()
                .Select(f => f.Name);
        }

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
                    var dateTime = alarm.DateTimeOffset.Date.Add(alarm.Time);
                    if (!CheckAlarmDateTime(dateTime)) return;

                    CreateNotification(alarm.Id, alarm.AudioFilename, alarm.ImageFilename, dateTime.ToUniversalTime());
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

        /// <summary>
        /// Plans future alarms.
        /// If it is already planned then nothing happens.
        /// </summary>
        /// <param name="alarm"></param>
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

                    // holidays should be ignores so we will check for them.
                    // It applies only for occurenes which are set to "repeatedly"
                    if (alarm.Occurrence == OccurrenceType.Repeatedly && alarm.IgnoreHolidays)
                    {
                        // TODO: handle local time/date time offcet!!! use offset!!!
                        var holiday = BaseAlarmSettings.Instance.GetHoliday(plannedDate.Date);
                        if (holiday != null) // holiday exists => do not set alarm
                        {
                            DateTime holidayInformDateTime;
                            if (holiday.Date.Date == DateTime.Now.Date) // holiday is today => inform immediatelly
                                holidayInformDateTime = DateTime.Now.AddMinutes(1);
                            else if (holiday.Date.Date == DateTime.Now.Date.AddDays(1))
                                holidayInformDateTime = DateTime.Now.AddMinutes(1); // holiday is tomorrow => inform immediatelly so we can change it if required
                            else // more than one day to holiday => plan message
                                holidayInformDateTime = holiday.Date.AddDays(-1); // inform us one day before holiday

                            var message = $"Alarm not set for {holiday.Date.Date.ToString("MM.dd.yyyy")} because that day is holiday.";
                            var message2 = $"Holiday description: {holiday.LocalDescription}";
                            CreateHolidayNotification(alarm.Id, "Alarm -> Holiday", message, message2, holidayInformDateTime);

                            continue;
                        }
                    }

                    // either holiday is not found (that day is not holiday or holidays are not laoded) or occurenc is set to "once"
                    CreateNotification(alarm.Id, alarm.AudioFilename, alarm.ImageFilename, dateTime.ToUniversalTime());
                }
            }
        }

        public void CreateNotification(int alarmId, string audioFile, string imageFile, DateTime dateTime)
        {
            var audioFileUriString = $"ms-appdata:///local/{AudioFolderName}/{audioFile}"; // file is copied to local folder so we use different prefix
            var toastXmlContent = CreateToastXml(ToastDuration.Long, audioFileUriString, false, imageFile, alarmId.ToString());
            var xmlString = toastXmlContent.GetXml();

            var toast = new ScheduledToastNotification(toastXmlContent,
                dateTime.ToUniversalTime(),
                TimeSpan.FromMinutes(1),
                1);
            toast.Id = alarmId.ToString(); // not unique. It is enouqh to identify alarm not "toast"
            ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alarmId">Alarm id which is used to identify the toast. Used when alarm is being disabled so that toast can be removed.</param>
        /// <param name="caption"></param>
        /// <param name="message"></param>
        /// <param name="message2"></param>
        /// <param name="holidayInformDateTime"></param>
        private void CreateHolidayNotification(int alarmId, string caption, string message, string message2, DateTime holidayInformDateTime)
        {
            var toastXmlContent = CreateToastMessageXml(caption, message, message2);

            var toast = new ScheduledToastNotification(toastXmlContent,
                holidayInformDateTime.ToUniversalTime(),
                TimeSpan.FromMinutes(1),
                1);
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
        /// <summary>
        /// TODO: access from different classes
        /// </summary>
        public const string ImagesFolderName = "Images";

        private XmlDocument CreateToastXml(ToastDuration duration, string audioPath, bool loop, string imageFilename, string alarmAction)
        {
            var snoozeTimeId = "snoozeTimeId";
            ToastContent content = new ToastContent()
            {
                Duration = duration,
                Scenario = ToastScenario.Alarm,
                Launch = "app-defined-string",
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = "Alarm", HintAlign= AdaptiveTextAlign.Center, HintStyle = AdaptiveTextStyle.Title },
                            new AdaptiveText() { Text= "Wake up! Wake up!" },
                            new AdaptiveImage() { Source = $"ms-appdata:///local/Images/{imageFilename}" }
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Inputs =
                    {
                        new ToastSelectionBox(snoozeTimeId)
                        {
                            DefaultSelectionBoxItemId  = "10",
                            Items =
                            {
                                new ToastSelectionBoxItem("5", "5 minutes"),
                                new ToastSelectionBoxItem("10", "10 minutes"),
                                new ToastSelectionBoxItem("15", "15 minutes"),
                                new ToastSelectionBoxItem("30", "30 minutes"),
                                new ToastSelectionBoxItem("60", "1 hour")
                            }
                        }
                    },
                    Buttons =
                    {
                        new ToastButton("Configure Alarm", $"alarmAction;{alarmAction}") { ImageUri = "check.png" },
                        //new ToastButtonSnooze() {  SelectionBoxId = snoozeTimeId },
                        //new ToastButtonDismiss()
                        new ToastButton("My Snooze",$"snooze:{alarmAction}") {ActivationType = ToastActivationType.Background  },
                        new ToastButton("My Dismiss", $"dismiss:{alarmAction}") {ActivationType = ToastActivationType.Background }
                    }
                },
                Audio = new ToastAudio() { Src = new Uri(audioPath), Loop = loop }
            };

            return content.GetXml();
        }

        private XmlDocument CreateToastMessageXml(string caption, string text, string text2)
        {
            ToastContent content = new ToastContent()
            {
                Duration = ToastDuration.Long,
                Scenario = ToastScenario.Default,
                Launch = "app-defined-string",
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = caption },
                            new AdaptiveText() { Text= text },
                            new AdaptiveText() { Text= text2 }
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                }
            };

            return content.GetXml();
        }
    }
}
