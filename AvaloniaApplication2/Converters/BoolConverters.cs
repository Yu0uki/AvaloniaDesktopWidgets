using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AvaloniaApplication2.Converters
{
    /// <summary>
    /// 布尔值到字符串转换器，用于根据布尔值显示不同的文本
    /// </summary>
    public class BoolToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string param)
            {
                var parts = param.Split('|');
                if (parts.Length == 2)
                {
                    return boolValue ? parts[0] : parts[1];
                }
            }
            return value?.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 布尔值到画刷转换器，用于根据布尔值显示不同的颜色
    /// </summary>
    public class BoolToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string param)
            {
                var parts = param.Split('|');
                if (parts.Length == 2)
                {
                    var colorStr = boolValue ? parts[0] : parts[1];
                    return Brush.Parse(colorStr);
                }
            }
            return Brushes.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 整数到布尔值转换器，用于根据集合数量判断是否可见，或比较两个整数是否相等
    /// </summary>
    public class IntToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int count && parameter is string param)
            {
                // 如果参数是数字，则比较是否相等
                if (int.TryParse(param, out int compareValue))
                {
                    return count == compareValue;
                }
                // 否则判断是否大于 0
                return count > 0;
            }
            if (value is int countNoParam)
            {
                return countNoParam > 0;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
