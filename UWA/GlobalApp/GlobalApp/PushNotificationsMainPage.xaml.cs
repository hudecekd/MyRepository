using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GlobalApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PushNotificationsMainPage : Page
    {
        public PushNotificationsMainPage()
        {
            this.InitializeComponent();

            this.DataContext = PushNotificationsVM.Notifications;

            this.chbPassToBT.IsChecked = PushNotificationsVM.PassToBackgroundTask;
        }

        private void chbPassToBT_Checked(object sender, RoutedEventArgs e)
        {
            PushNotificationsVM.PassToBackgroundTask = true;
        }

        private void chbPassToBT_Unchecked(object sender, RoutedEventArgs e)
        {
            PushNotificationsVM.PassToBackgroundTask = false;
        }
    }
}
