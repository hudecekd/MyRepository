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

        private Dictionary<TextBlock, Point> locations = new Dictionary<TextBlock, Point>();

        private void UserControl_Loading(FrameworkElement sender, object args)
        {
            // TODO: baseY + set locations of rectangles and other elemnents.!!!
            var baseY = 202;
            var r = 190;
            for (var hour = 1; hour <= 12; hour++)
            {
                var alpha = 360d / 12 * hour;
                var alphaR = alpha / 180 * Math.PI;
                var left = Math.Sin(alphaR) * r;
                var top = -Math.Cos(alphaR) * r; // minus since we need it because of cosinus.
                TextBlock tbHour = new TextBlock();
                tbHour.Text = hour.ToString();

                // left & top is baseY
                // then + half of base rectangle
                // for now it is ok
                // TODO: make all coordinates better!!!
                var centerY = baseY + 13 / 2;

                var newPoint = new Point();// new Point(left + 189, top + centerY);

                //tbHour.Margin = new Thickness(left + 189, top + baseY, 0, 0);

                tbHour.HorizontalAlignment = HorizontalAlignment.Left;
                tbHour.VerticalAlignment = VerticalAlignment.Top;

                tbHour.SetValue(Canvas.LeftProperty, newPoint.X);
                tbHour.SetValue(Canvas.TopProperty, newPoint.Y);

                //gClock.Children.Add(tbHour);
                cClock.Children.Add(tbHour);

                locations.Add(tbHour, new Point(left + 189, top + centerY));
            }

            for (var minutes = 1; minutes <= 60; minutes++)
            {
                var alpha = 360d / 60 * minutes;
                var alphaR = alpha / 180 * Math.PI;

                // if minutes can represent hour then make it longer
                var width = (minutes % 5 == 0) ? 20 : 10;

                // base y location + base rectangles height /2 (which gives us
                // center y location and then
                // minus half of height which should make new recangle centered
                // following rotation should be ok because it shuld be correctly centered
                // because of base rectangle definition.
                var y = baseY + 13 / 2 - 5 / 2;

                var rectangle = new Rectangle();
                rectangle.Margin = new Thickness(189 + 160, y, 0, 0);
                rectangle.HorizontalAlignment = HorizontalAlignment.Left;
                rectangle.VerticalAlignment = VerticalAlignment.Top;
                rectangle.Width = width;
                rectangle.Height = 5;
                // TODO: use system color as for textbox
                // on desktop it shoul be black and on mobile
                // where background is black it should be white
                rectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.White);
                var transform = new RotateTransform();
                transform.Angle = alpha - 90;
                transform.CenterX = -160;
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
            CenterHourTexts();
        }

        private void CenterHourTexts()
        {
            foreach (var item in locations)
            {
                item.Key.SetValue(CenterOnPoint.CenterPointProperty, item.Value);
            }
        }

        private void btnCenter_Click(object sender, RoutedEventArgs e)
        {
            CenterHourTexts();
        }
    }
}
