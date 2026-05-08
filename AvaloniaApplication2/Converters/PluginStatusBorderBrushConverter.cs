using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AvaloniaApplication2.Converters
{
    /// <summary>
    /// Converts plugin (IsEnabled, IsRunning) to a border brush for the status indicator.
    /// Expects MultiBinding with IsEnabled (bool) and IsRunning (bool) in order.
    /// </summary>
    public class PluginStatusBorderBrushConverter : IMultiValueConverter
    {
        private static readonly SolidColorBrush GreenBrush = new(Color.FromRgb(16, 124, 16));
        private static readonly SolidColorBrush YellowBrush = new(Color.FromRgb(255, 185, 0));
        private static readonly SolidColorBrush GrayBrush = new(Color.FromRgb(224, 224, 224));

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2) return GrayBrush;

            bool isEnabled = values[0] is true;
            bool isRunning = values[1] is true;

            if (!isEnabled) return GrayBrush;
            if (isRunning) return GreenBrush;
            return YellowBrush; // enabled but not running (paused)
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
