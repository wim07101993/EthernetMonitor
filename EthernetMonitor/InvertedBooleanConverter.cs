using System;
using System.Globalization;
using System.Windows.Data;

namespace EthernetMonitor
{
    public class InvertedBooleanConverter : IValueConverter
    {
        private static InvertedBooleanConverter instance;
        public static InvertedBooleanConverter Instance => instance ??= new InvertedBooleanConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b && !b;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b && !b;
    }
}
