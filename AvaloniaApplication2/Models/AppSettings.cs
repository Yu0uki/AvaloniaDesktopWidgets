using System.Collections.Generic;

namespace AvaloniaApplication2.Models
{
    public class AppSettings
    {
        public string Theme { get; set; } = "Light";
        public bool AutoLoadPlugins { get; set; } = true;
        public string PluginsDirectory { get; set; } = "./Plugins";
        public List<string> EnabledPlugins { get; set; } = new();
        public double WindowWidth { get; set; } = 1200;
        public double WindowHeight { get; set; } = 750;

        // 界面设置
        public int DefaultStartupPageIndex { get; set; }
        public int LanguageIndex { get; set; } = 1;
        public int AccentColorIndex { get; set; }
        public double BackgroundOpacity { get; set; } = 85;
        public bool CustomBackgroundImage { get; set; }
        public string BackgroundImagePath { get; set; } = "";
    }
}
