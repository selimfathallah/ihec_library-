using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace IHECLibrary.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string colors)
            {
                string[] colorOptions = colors.Split(',');
                string selectedColor = boolValue ? colorOptions[0] : colorOptions.Length > 1 ? colorOptions[1] : colorOptions[0];
                
                if (selectedColor.StartsWith("#") && Color.TryParse(selectedColor, out var parsedColor))
                {
                    return parsedColor;
                }
                
                return selectedColor;
            }

            return Colors.Black;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}