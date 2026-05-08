using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AvaloniaApplication2.Models
{
    public class AppSettings
    {
        public int ConfigVersion { get; set; } = 1;
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

        // UI布局持久化
        public bool SidebarExpanded { get; set; }
        public double SidebarWidth { get; set; } = 64;
        public double? WindowX { get; set; }
        public double? WindowY { get; set; }

        // 插件元数据 (PluginId → JSON)
        public Dictionary<string, JsonElement> PluginSettings { get; set; } = new();

        // 通知历史 (最近50条)
        public List<NotificationLogEntry> NotificationLog { get; set; } = new();
    }

    public class NotificationLogEntry
    {
        public string Message { get; set; } = "";
        public string Type { get; set; } = "Info"; // Info, Success, Warning, Error
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
