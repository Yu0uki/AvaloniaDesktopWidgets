using System.Collections.Generic;

namespace AvaloniaApplication2.Models
{
    /// <summary>
    /// 应用程序设置模型
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 主题设置：Light, Dark, System
        /// </summary>
        public string Theme { get; set; } = "Light";

        /// <summary>
        /// 启动时自动加载插件
        /// </summary>
        public bool AutoLoadPlugins { get; set; } = true;

        /// <summary>
        /// 插件目录路径（相对于应用程序运行目录）
        /// </summary>
        public string PluginsDirectory { get; set; } = "./Plugins";

        /// <summary>
        /// 已启用的插件ID列表
        /// </summary>
        public List<string> EnabledPlugins { get; set; } = new();

        /// <summary>
        /// 窗口宽度
        /// </summary>
        public double WindowWidth { get; set; } = 1200;

        /// <summary>
        /// 窗口高度
        /// </summary>
        public double WindowHeight { get; set; } = 750;
    }
}
