using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AvaloniaApplication2.Converters
{
    /// <summary>
    /// 日志类型 → 背景色转换
    /// </summary>
    public class LogTypeColorConverter : IValueConverter
    {
        private static readonly SolidColorBrush ErrorBrush   = new(Color.FromRgb(214, 50, 56));
        private static readonly SolidColorBrush WarnBrush    = new(Color.FromRgb(255, 185, 0));
        private static readonly SolidColorBrush SuccessBrush = new(Color.FromRgb(16, 124, 16));
        private static readonly SolidColorBrush InfoBrush    = new(Color.FromRgb(0, 120, 212));

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (value as string) switch
            {
                "Error"   => ErrorBrush,
                "Warning" => WarnBrush,
                "Success" => SuccessBrush,
                _         => InfoBrush
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
