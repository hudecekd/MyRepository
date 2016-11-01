using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace GlobalApp.Converters.Alarm
{
    public class DaysToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return DependencyProperty.UnsetValue;
            var alarmVM = value as AlarmSettingVM;

            if (alarmVM.OnlyOnce) return alarmVM.DateTimeOffset.Date.ToString("MM.dd.yyyy");

            // repeatedly
            var days = new List<string>();
            if (alarmVM.UseMonday) days.Add("Mon");
            if (alarmVM.UseTuesday) days.Add("Tue");
            if (alarmVM.UseWednesday) days.Add("Wed");
            if (alarmVM.UseThursday) days.Add("Thu");
            if (alarmVM.UseFriday) days.Add("Fri");
            if (alarmVM.UseSaturday) days.Add("Sat");
            if (alarmVM.UseSunday) days.Add("Sun");
            return string.Join(", ", days);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException(); // not used
        }
    }
}
