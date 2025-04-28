using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace IHECLibrary.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string options)
            {
                string[] textOptions = options.Split(',');
                return boolValue ? textOptions[0] : textOptions.Length > 1 ? textOptions[1] : textOptions[0];
            }

            return "N/A";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}