using AlarmLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace GlobalApp.Converters.Alarm
{
    public class OccurrenceToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return DependencyProperty.UnsetValue;

            var occurrence = (OccurrenceType)value;

            switch (occurrence)
            {
                case OccurrenceType.OnlyOnce:
                    return "Only once";
                case OccurrenceType.Repeatedly:
                    return "Repeatedly";
            }

            return occurrence.ToString(); // should never occur
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException(); // not used
        }
    }
}
