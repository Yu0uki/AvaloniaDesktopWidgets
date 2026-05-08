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
        public bool SidebarExpanded { get; set; } = true;
        public double SidebarWidth { get; set; } = 220;
        public double? WindowX { get; set; }
        public double? WindowY { get; set; }

        // 搜索
        public bool EnableSearchHistory { get; set; } = true;
        public List<string> SearchHistory { get; set; } = new();

        // 插件市场
        public string PluginMarketUrl { get; set; } = "https://raw.githubusercontent.com/Yu0uki/EfficiencyWorkshop.Plugins/main/index.json";

        // 设置同步 (GitHub)
        public bool SyncEnabled { get; set; }
        public bool AutoSync { get; set; }
        public string SyncRepoOwner { get; set; } = "";
        public string SyncRepoName { get; set; } = "";
        public string SyncToken { get; set; } = "";
        public string SyncBranch { get; set; } = "main";
        public string? LastSyncTime { get; set; }

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

        public string TypeDisplay => Type switch
        {
            "Error" => "错误",
            "Warning" => "警告",
            "Success" => "成功",
            _ => "信息"
        };

        public string TypeColor => Type switch
        {
            "Error" => "#D13438",
            "Warning" => "#FFB900",
            "Success" => "#107C10",
            _ => "#0078D4"
        };

        public string TypeIcon => Type switch
        {
            "Error" => "",
            "Warning" => "",
            "Success" => "",
            _ => ""
        };
    }
}
