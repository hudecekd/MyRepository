using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GlobalApp
{
    public sealed partial class ClockUC : UserControl
    {
        private DispatcherTimer _timer = new DispatcherTimer();
        public ClockUC()
        {
            this.InitializeComponent();

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private DateTime _previousTime;

        private void _timer_Tick(object sender, object e)
        {
            UpdateClock();
        }

        private void UpdateClock()
        { 
            var time = DateTime.Now;

            var hourShift = (1d * time.Minute * 60 + time.Second) / 3600; // how close we are to the next hour

            txtDateTime.Text = time.ToString();
            rtClockSeconds.Angle = (time.Second - 15) * 6; // decrease 15 seconds since default rectangle position is at 15 seconds (45 degrees).
            rtClockMinutes.Angle = (time.Minute - 15) * 6;
            rtClockHours.Angle = (time.Hour % 12 + hourShift) * 360 / 12 - 90; // divide by 12 and take the rest since clock has 12 digits.

            // update tile every minute
            if (time.Minute != _previousTime.Minute)
            {
                //UpdateTile(time.ToString("MM.dd.yyyy hh:mm", System.Globalization.CultureInfo.InvariantCulture));
            }

            _previousTime = time;
        }

        private void UserControl_Loading(FrameworkElement sender, object args)
        {
            var r = 160;
            for (var hour = 1; hour <= 12; hour++)
            {
                var alpha = 360d / 12 * hour;
                var alphaR = alpha / 180 * Math.PI;
                var left = Math.Sin(alphaR) * r;
                var top = -Math.Cos(alphaR) * r; // minus since we need it because of cosinus.
                TextBlock tbHour = new TextBlock();
                tbHour.Text = hour.ToString();
                tbHour.Margin = new Thickness(left + 189, top + 152, 0, 0);

                gClock.Children.Add(tbHour);
            }

            for (var minutes = 1; minutes <= 60; minutes++)
            {
                var alpha = 360d / 60 * minutes;
                var alphaR = alpha / 180 * Math.PI;

                // if minutes can represent hour then make it longer
                var width = (minutes % 5 == 0) ? 20 : 10;

                var rectangle = new Rectangle();
                rectangle.Margin = new Thickness(189 + 156, 152, 0, 0);
                rectangle.HorizontalAlignment = HorizontalAlignment.Left;
                rectangle.VerticalAlignment = VerticalAlignment.Top;
                rectangle.Width = width;
                rectangle.Height = 5;
                rectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.Black);
                var transform = new RotateTransform();
                transform.Angle = alpha - 90;
                transform.CenterX = -156;
                transform.CenterY = 0;
                rectangle.RenderTransform = transform;

                gClock.Children.Add(rectangle);
            }
        }

        public static void UpdateTile(string infoString)
        {         // create the instance of Tile Updater, which enables you to change the appearance of the calling app's tile         
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            // enables the tile to queue up to five notifications         
            updater.EnableNotificationQueue(true);
            updater.Clear();
            // get the XML content of one of the predefined tile templates, so that, you can customize it         
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText04);
            tileXml.GetElementsByTagName("text")[0].InnerText = infoString;
            // Create a new tile notification.         

            var tileNotification = new TileNotification(tileXml);
            tileNotification.ExpirationTime = DateTime.Now.AddSeconds(10);
            updater.Update(tileNotification);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateClock(); // first time we will not wait for timer and update the clock immediatelly
        }
    }
}
