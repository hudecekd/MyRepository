using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Sms;
using Windows.Devices.Spi;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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
    public sealed partial class SmsMainPage : Page
    {
        public SmsMainPage()
        {
            this.InitializeComponent();
        }

        private async void btnGetDeviceInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selector = SmsDevice2.GetDeviceSelector();
                await new MessageDialog("before").ShowAsync();
                var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(selector);
                await new MessageDialog("after").ShowAsync();

                lbDevices.Items.Clear();
                foreach (var device in devices)
                {
                    lbDevices.Items.Add(device.Id);
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void btnSendSms_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var id = lbDevices.SelectedItem as string;
                var device = SmsDevice2.FromId(id);
                var message = new SmsTextMessage2();
                message.Body = "test sms";
                var messageLength = device.CalculateLength(message);
                message.To = txtTo.Text;
                var result = await device.SendMessageAndGetResultAsync(message);

                await new MessageDialog($"successfull: {result.IsSuccessful}").ShowAsync();
                await new MessageDialog($"TransportFailureCause: {result.TransportFailureCause}").ShowAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }
    }
}
