using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Profile;
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
    public sealed partial class ApplicationInfo : UserControl
    {
        public ApplicationInfo()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var dfv = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            ulong v1 = (dfv & 0xFFFF000000000000L) >> 48;
            ulong v2 = (dfv & 0x0000FFFF00000000L) >> 32;
            ulong v3 = (dfv & 0x00000000FFFF0000L) >> 16;
            ulong v4 = (dfv & 0x000000000000FFFFL);
            string deviceFamilyVersion = $"{v1}.{v2}.{v3}.{v4}";

            tbDeviceForm.Text = AnalyticsInfo.DeviceForm;
            tbDeviceFamily.Text = AnalyticsInfo.VersionInfo.DeviceFamily;
            tbDeviceFamilyVersion.Text = deviceFamilyVersion;

            var version = Windows.ApplicationModel.Package.Current.Id.Version;
            tbPackageVersion.Text = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            tbDataVersion.Text = Windows.Storage.ApplicationData.Current.Version.ToString();
        }
    }
}
