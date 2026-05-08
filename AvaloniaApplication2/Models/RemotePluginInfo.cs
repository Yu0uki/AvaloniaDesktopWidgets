using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace AvaloniaApplication2.Models
{
    /// <summary>
    /// 远程插件市场中的插件信息
    /// </summary>
    public partial class RemotePluginInfo : ObservableObject
    {
        [ObservableProperty]
        private string id = "";

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string author = "";

        [ObservableProperty]
        private string description = "";

        [ObservableProperty]
        private string version = "";

        [ObservableProperty]
        private DateTime updatedAt;

        [ObservableProperty]
        private string downloadUrl = "";

        [ObservableProperty]
        private string icon = "📦";

        [ObservableProperty]
        private string[] tags = Array.Empty<string>();

        /// <summary>本地是否已安装</summary>
        [ObservableProperty]
        private bool isInstalled;

        /// <summary>安装中状态</summary>
        [ObservableProperty]
        private bool isInstalling;

        /// <summary>安装进度文本</summary>
        [ObservableProperty]
        private string installStatus = "";

        public string UpdatedDisplay
        {
            get
            {
                var span = DateTime.Now - UpdatedAt;
                return span switch
                {
                    { TotalDays: >= 365 } => $"更新于 {span.TotalDays / 365:F0} 年前",
                    { TotalDays: >= 30 } => $"更新于 {span.TotalDays / 30:F0} 月前",
                    { TotalDays: >= 1 } => $"更新于 {span.TotalDays:F0} 天前",
                    { TotalHours: >= 1 } => $"更新于 {span.TotalHours:F0} 小时前",
                    { TotalMinutes: >= 1 } => $"更新于 {span.TotalMinutes:F0} 分钟前",
                    _ => "刚刚更新"
                };
            }
        }
    }

    /// <summary>
    /// GitHub 插件市场索引文件格式
    /// </summary>
    public class PluginMarketIndex
    {
        public string name { get; set; } = "Efficiency Workshop Plugin Market";
        public PluginMarketEntry[] plugins { get; set; } = Array.Empty<PluginMarketEntry>();
    }

    public class PluginMarketEntry
    {
        public string id { get; set; } = "";
        public string name { get; set; } = "";
        public string author { get; set; } = "";
        public string description { get; set; } = "";
        public string version { get; set; } = "";
        public string updatedAt { get; set; } = "";
        public string downloadUrl { get; set; } = "";
        public string icon { get; set; } = "📦";
        public string[] tags { get; set; } = Array.Empty<string>();
    }
}
