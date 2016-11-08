using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
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
    public sealed partial class GpsMainPage : Page
    {
        Geolocator gl = new Geolocator();

        public GpsMainPage()
        {
            this.InitializeComponent();

            gl.DesiredAccuracyInMeters = 50;
            gl.PositionChanged += Gl_PositionChanged;
        }

        private double _latitude;
        private double _longitude;

        private BasicGeoposition _previousPoint;
        private DateTime _previousDateTime;

        private async void Gl_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txtLocation.Text = "GPS:" + args.Position.Coordinate.Point.Position.Latitude.ToString("0.0000") + ", " + args.Position.Coordinate.Point.Position.Longitude.ToString("0.0000");
                dtpLastUpdate.Text = DateTime.Now.ToString("MM.dd.yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                var distance = GetDistance(_previousPoint, args.Position.Coordinate.Point.Position);
                var currentDateTime = DateTime.Now;

                // do not show speed for the first time
                if (_previousDateTime != default(DateTime))
                {
                    var speed = GetSpeed(distance, _previousDateTime, currentDateTime);
                    txtCurrentSpeed.Text = string.Format("{0:0.00} km/h", speed);
                }

                _previousDateTime = currentDateTime;
                _previousPoint = args.Position.Coordinate.Point.Position;

                if ((Math.Abs( args.Position.Coordinate.Point.Position.Latitude - _latitude) < 0.05) &&
                    (Math.Abs(args.Position.Coordinate.Point.Position.Longitude - _longitude) < 0.05))
                {
                    return; // small change => ignore it.
                }

                // show last remembered location
                txtLastLocation.Text = txtLocation.Text;

                _latitude = args.Position.Coordinate.Point.Position.Latitude;
                _longitude = args.Position.Coordinate.Point.Position.Longitude;

                
                if (lbGpsHistory.Items.Count == 100)
                {
                    lbGpsHistory.Items.RemoveAt(lbGpsHistory.Items.Count - 1);
                }
                lbGpsHistory.Items.Insert(0, txtLocation.Text);
            });
        }

        private async void btnGetGPS_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var gp = await gl.GetGeopositionAsync(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10));
                txtLocation.Text = "GPS:" + gp.Coordinate.Point.Position.Latitude.ToString("0.00") + ", " + gp.Coordinate.Point.Position.Longitude.ToString("0.00");
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private double GetDistance(BasicGeoposition a, BasicGeoposition b)
        {
            var lat1 = a.Latitude * Math.PI / 180;
            var lon1 = a.Longitude * Math.PI / 180;

            var lat2 = a.Latitude * Math.PI / 180;
            var lon2 = a.Longitude * Math.PI / 180;

            double r = 6378100; // earth

            double rho1 = r * Math.Cos(lat1);
            double z1 = r * Math.Sin(lat1);
            double x1 = rho1 * Math.Cos(lon1);
            double y1 = rho1 * Math.Sin(lon1);

            double rho2 = r * Math.Cos(lat2);
            double z2 = r * Math.Sin(lat2);
            double x2 = rho1 * Math.Cos(lon2);
            double y2 = rho1 * Math.Sin(lon2);

            var scalar = x1 * x2 + y1 * y2 + z1 * z2;
            var absA = r;
            var absB = r;
            var cosAlpha = scalar / (absA * absB);
            var Alpha = Math.Acos(cosAlpha);

            return r * Alpha;
        }

        public double GetSpeed(double distance, DateTime start, DateTime end)
        {
            var deltaTime = end - start;
            return distance / deltaTime.TotalSeconds * 3.6; // km/h
        }
    }
}
