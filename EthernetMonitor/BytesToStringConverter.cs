using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace EthernetMonitor
{
    public class BytesToStringConverter : IValueConverter
    {
        private static BytesToStringConverter instance;
        public static BytesToStringConverter Instance => instance ??= new BytesToStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as IEnumerable<byte>)
                ?.Aggregate(new StringBuilder(), (b, x) => b.Append($"{x:X} "))
                .ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string s))
                return value;

            var split = s.Split(new char[] { ',', ' ', ';', '/', '-', '_', '.' }, StringSplitOptions.RemoveEmptyEntries);
            return split.Select(x => 
                {
                    var success = byte.TryParse(x, out var value);
                    return (value, success);
                })
                .Where(x => x.success)
                .Select(x => x.value)
                .ToArray();
        }
    }
}
