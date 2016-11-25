using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Networking.PushNotifications;
using Microsoft.WindowsAzure.Messaging;
using Windows.UI.Popups;

using AlarmLibrary;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace GlobalApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static App CurrentApp
        {
            get { return (App)App.Current; }
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.EnteredBackground += Application_EnteredBackground;
            this.Resuming += App_Resuming;

            // App was started again. We have to register again to get notifications that BT has completed.
            BackgroundTaskHelper.TryRegisterCompletedHandler("AlarmToastBackgroundTask", (s, e) =>
            {
                App.CurrentApp.OnToastBackgroundTaskCompleted();
            });

            // If usre installs app for the first time and "application data version"
            // is already greater than 0 then we need to set that version
            // so we will call "update" method.
            // If app was already installed before then registered BT which is called
            // after application update will handle the future updates of application data.
            AlarmLibrary.AppVersion.UpdateSettings();

            // register push notifications
            // first before connection check.
            // TODO: improve this mechanism
            // HOTFIX: for now ignore possible errors
            try
            {
                Task.Factory.StartNew(() => InitNotificationsAsync().GetAwaiter().GetResult()).Wait();
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message).ShowAsync();
            }
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

            AlarmManager.Instance.CopyAudioFiles();
            AlarmManager.Instance.CopyImages();
            BaseAlarmSettings.Instance.LoadSettings();
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            try
            {
                var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
                if (connectionProfile != null)
                {
                    if (connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess)
                    {
                        var registered = InitNotificationsAsync().GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: handle exception
            }
        }

        public void OnToastBackgroundTaskCompleted()
        {
            ToastBackgroundTaskCompleted(null, EventArgs.Empty);
        }

        public event EventHandler ToastBackgroundTaskCompleted = (s, e) => { };

        private void App_Resuming(object sender, object e)
        {
            BaseAlarmSettings.Instance.LoadSettings();
        }

        private async Task<bool> InitNotificationsAsync()
        {
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

            channel.PushNotificationReceived += Channel_PushNotificationReceived;

            var hub = new NotificationHub("DusnaNH", "Endpoint=sb://dusnanhn.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=ERtJl2vJ/FAApBs7yyAM1kQhAVUM2tedNbeNu1vkPns=");
            var result = await hub.RegisterNativeAsync(channel.Uri);

            PushNotificationsVM.ChannelUri = channel.Uri;

            // Displays the registration ID so you know it was successful
            if (result.RegistrationId != null)
            {
                return true;
            }
            return false;
        }

        private void Channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            args.Cancel = !PushNotificationsVM.PassToBackgroundTask;

            var notificationVM = new PushNotificationVM();
            notificationVM.DateTime = DateTime.Now;
            notificationVM.Type = args.NotificationType;
            switch (args.NotificationType)
            {
                case PushNotificationType.Raw:
                    notificationVM.Notification = args.RawNotification.Content;
                    break;
                case PushNotificationType.Toast:
                    var content = args.ToastNotification.Content.GetXml();
                    if (content.Length > 100) content = content.Substring(0, 100);
                    notificationVM.Notification = content;
                    break;
                default:
                    // for now ignore other types
                    break;
            }

            // WARNING: use or do not use await?
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
             {
                 if (PushNotificationsVM.Notifications.Count == 100)
                     PushNotificationsVM.Notifications.RemoveAt(PushNotificationsVM.Notifications.Count - 1);
                 PushNotificationsVM.Notifications.Insert(0, notificationVM);
             });
        }

        protected async override void OnActivated(IActivatedEventArgs args)
        {
            // simulate start of app. Same as OnLaunched method.
            OnLaunched2(ApplicationExecutionState.NotRunning, false, "");

            try
            {
                if (args.Kind == ActivationKind.ToastNotification)
                {
                    // TODO: distinguish different toasts (alarm, holiday and others)
                    var toastArgs = args as ToastNotificationActivatedEventArgs;
                    var alarmIdStr = toastArgs.Argument;
                    var alarmId = int.Parse(alarmIdStr);
                    var snoozeTimeStr = toastArgs.UserInput["snoozeTimeId"].ToString();
                    var snoozeTimeSeconds = int.Parse(snoozeTimeStr);

                    var alarm = BaseAlarmSettings.Instance.Alarms.Single(a => a.Id == alarmId);
                    var dateTime = DateTimeOffset.Now.DateTime.Add(TimeSpan.FromMinutes(snoozeTimeSeconds));
                    //if (!CheckAlarmDateTime(dateTime)) return;

                    AlarmManager.Instance.CreateNotification(alarm.Id, alarm.AudioFilename, alarm.ImageFilename, dateTime.ToUniversalTime());
                }


                //if (args.Kind == ActivationKind.ToastNotification)
                //{
                //    var toastArgs = args as ToastNotificationActivatedEventArgs;
                //    var argumentParts = toastArgs.Argument.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                //    var argumentAction = argumentParts[0];
                //    var alarmId = int.Parse(argumentParts[1]);

                //    var rootFrame = Window.Current.Content as Frame;
                //    var content = rootFrame.Content;
                //    var mainPage = content as MainPage;
                //    mainPage.OpenAlarmSetting(alarmId);
                //}

                base.OnActivated(args);
            }
            catch (Exception ex)
            {
                var message = ex.ToString().Substring(0, 1024);
                await new Windows.UI.Popups.MessageDialog(message).ShowAsync();
            }
        }

        private void OnLaunched2(ApplicationExecutionState previousExecutionState, bool prelaunchActivated, object arguments)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (previousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (prelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity

            BaseAlarmSettings.Instance.SaveSettings();


            deferral.Complete();
        }

        private void Application_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            //ClockUC.UpdateTile("Entered Background");
        }
    }
}
