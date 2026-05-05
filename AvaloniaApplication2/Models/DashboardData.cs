using System;
using System.Collections.Generic;

namespace AvaloniaApplication2.Models
{
    /// <summary>
    /// 仪表盘数据持久化模型
    /// </summary>
    public class DashboardData
    {
        public string SelectedCity { get; set; } = "北京市";
        public List<DashboardTodoItem> TodoItems { get; set; } = new();
        public List<DashboardQuickApp> QuickApps { get; set; } = new();
        public List<DashboardClipboardItem> ClipboardHistory { get; set; } = new();
        public DashboardLayout Layout { get; set; } = new();
    }

    public class DashboardLayout
    {
        public bool ShowWeatherCard { get; set; } = true;
        public bool ShowQuickAppsCard { get; set; } = true;
        public bool ShowSystemMonitorCard { get; set; } = true;
        public bool ShowTodoCard { get; set; } = true;
        public bool ShowClipboardCard { get; set; } = true;
        public bool ShowPluginStatsCard { get; set; } = true;
    }

    public class DashboardTodoItem
    {
        public string Title { get; set; } = "";
        public bool IsCompleted { get; set; }
    }

    public class DashboardQuickApp
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
    }

    public class DashboardClipboardItem
    {
        public string Content { get; set; } = "";
        public DateTime Time { get; set; }
    }
}
