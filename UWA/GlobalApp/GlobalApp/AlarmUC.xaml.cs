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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GlobalApp
{
    public sealed partial class AlarmUC : UserControl, IBackButtonNavigation
    {
        public AlarmUC()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var pageType = typeof(AlarmMainPage);
            frameAlarm.Navigate(pageType);
        }

        public bool NavigateBack()
        {
            if (frameAlarm.CanGoBack)
            {
                frameAlarm.GoBack();
                return true;
            }

            return false;
        }
    }
}
