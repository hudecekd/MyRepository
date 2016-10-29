using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GlobalApp
{
    public sealed partial class TileUpdate : UserControl
    {
        private const string TaskName = "TileUpdateBackgroundTask";
        private const string TaskAssemblyName = "TileUpdateBackgroundTask";

        private const string AlarmTaskName = "AlarmBackgroundTask";
        private const string AlarmAssemblyName = "AlarmBackgroundTask";

        public TileUpdate()
        {
            this.InitializeComponent();
        }

        private void btnPlanUpdates_Click(object sender, RoutedEventArgs e)
        {
            UpdateTile();
        }

        public static void UpdateTile()
        {         // create the instance of Tile Updater, which enables you to change the appearance of the calling app's tile         
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            // enables the tile to queue up to five notifications         
            updater.EnableNotificationQueue(true);
            updater.Clear();

            for (int i = 0; i < 15; i++)
            {
                // get the XML content of one of the predefined tile templates, so that, you can customize it         
                XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText04);
                tileXml.GetElementsByTagName("text")[0].InnerText = i.ToString();
                // Create a new tile notification.        

                var notification = new ScheduledTileNotification(tileXml, DateTime.Now.AddSeconds(10 + i * 10));
                notification.ExpirationTime = DateTime.Now.AddSeconds(10 + i * 10 + 5);

                updater.AddToSchedule(notification);
            }
        }

        private bool IsTaskRegistered(string taskName)
        {
            var taskRegistered = false;

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == TaskName)
                {
                    taskRegistered = true;
                    break;
                }
            }
            return taskRegistered;
        }

        private async void btnRegisterBackgroundTask_Click(object sender, RoutedEventArgs e)
        {
            await RegisterTimerBackgroundTask(TaskName, TaskAssemblyName, 15);
        }

        private async Task RegisterTimerBackgroundTask(string taskName, string taskAssemblyName, uint interval)
        { 
            var taskRegistered = IsTaskRegistered(taskName);

            if (!taskRegistered)
            {
                //required call
                var access = await BackgroundExecutionManager.RequestAccessAsync();

                tbRegistration.Text = "";
                //abort if access isn't granted
                if (access == BackgroundAccessStatus.DeniedByUser || access == BackgroundAccessStatus.DeniedBySystemPolicy)
                {
                    tbRegistration.Text = "Denied";
                    return;
                }
                tbRegistration.Text = "Allowed";

                var builder = new BackgroundTaskBuilder();

                builder.Name = taskName;
                builder.TaskEntryPoint = taskAssemblyName + "." + taskName;
                builder.SetTrigger(new TimeTrigger(interval, false));

                builder.Register();
            }
        }

        private void btnUnregisterBackgroundTask_Click(object sender, RoutedEventArgs e)
        {
            var taskRegistration = BackgroundTaskRegistration.AllTasks.Values.SingleOrDefault(t => t.Name == TaskName);
            if (taskRegistration != null)
            {
                taskRegistration.Unregister(false);

                // remove all scheduled tile notifications task has planned already
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                var scheduledNotifications = updater.GetScheduledTileNotifications();
                foreach (var scheduledNotification in scheduledNotifications)
                {
                    updater.RemoveFromSchedule(scheduledNotification);
                }
            }
        }

        private void btnSeeScheduled_Click(object sender, RoutedEventArgs e)
        {
            lbScheduledNotifications.Items.Clear();

            var updater = ToastNotificationManager.CreateToastNotifier();
            var scheduledNotifications = updater.GetScheduledToastNotifications();
            foreach (var scheduledNotification in scheduledNotifications)
            {
                lbScheduledNotifications.Items.Add(scheduledNotification.DeliveryTime.ToString());
            }
        }

        private void btnClearScheduled_Click(object sender, RoutedEventArgs e)
        {
            var updater = ToastNotificationManager.CreateToastNotifier();
            var scheduledNotifications = updater.GetScheduledToastNotifications();
            foreach (var scheduledNotification in scheduledNotifications)
            {
                updater.RemoveFromSchedule(scheduledNotification);
            }
        }

        private async void btnRegisterAlarmWatcher_Click(object sender, RoutedEventArgs e)
        {
            // TODO: change interval to at least 1 day
            await RegisterTimerBackgroundTask(AlarmTaskName, AlarmAssemblyName, 15);
        }
    }
}
