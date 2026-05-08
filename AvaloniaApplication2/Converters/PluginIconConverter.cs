using Avalonia.Data.Converters;
using AvaloniaApplication2.Core;
using System;
using System.Globalization;

namespace AvaloniaApplication2.Converters
{
    /// <summary>
    /// Maps plugin ID/Name to a distinct FluentIcon character for visual differentiation.
    /// </summary>
    public class PluginIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string id) return FluentIcons.DefaultPlugin;

            var lower = id.ToLowerInvariant();
            return lower switch
            {
                "notepadplugin" or "notepad" => FluentIcons.Notepad,
                "jsontoolbox" or "jsontoolboxplugin" => FluentIcons.JsonTool,
                "worldclock" or "worldclockplugin" => FluentIcons.WorldClock,
                "passwordgenerator" or "passwordgeneratorplugin" => FluentIcons.Password,
                "unitconverter" or "unitconverterplugin" => FluentIcons.UnitConverter,
                _ => FluentIcons.DefaultPlugin
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
