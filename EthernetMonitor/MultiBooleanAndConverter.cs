using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace EthernetMonitor
{
    public class MultiBooleanAndConverter : IMultiValueConverter
    {
        private static MultiBooleanAndConverter instance;
        public static MultiBooleanAndConverter Instance => instance ??= new MultiBooleanAndConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => values.OfType<bool>().All(x => x);

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
