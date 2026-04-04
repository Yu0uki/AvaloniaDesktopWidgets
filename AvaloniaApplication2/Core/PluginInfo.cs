using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApplication2.Core
{
    /// <summary>
    /// 插件信息模型 - 用于在宿主程序中管理插件元数据
    /// </summary>
    public partial class PluginInfo : ObservableObject
    {
        /// <summary>
        /// 插件唯一标识符
        /// </summary>
        [ObservableProperty]
        private string id = "";

        /// <summary>
        /// 插件显示名称
        /// </summary>
        [ObservableProperty]
        private string name = "";

        /// <summary>
        /// 插件版本号
        /// </summary>
        [ObservableProperty]
        private string version = "";

        /// <summary>
        /// 插件描述
        /// </summary>
        [ObservableProperty]
        private string description = "";

        /// <summary>
        /// 插件作者
        /// </summary>
        [ObservableProperty]
        private string author = "";

        /// <summary>
        /// 插件DLL文件路径
        /// </summary>
        [ObservableProperty]
        private string dllPath = "";

        /// <summary>
        /// 是否已启用
        /// </summary>
        [ObservableProperty]
        private bool isEnabled = true;

        /// <summary>
        /// 是否已加载到内存
        /// </summary>
        [ObservableProperty]
        private bool isLoaded = false;

        /// <summary>
        /// 安装日期
        /// </summary>
        [ObservableProperty]
        private DateTime installedDate = DateTime.Now;

        /// <summary>
        /// 错误信息（如果加载失败）
        /// </summary>
        [ObservableProperty]
        private string? error;

        /// <summary>
        /// 最后使用时间
        /// </summary>
        [ObservableProperty]
        private DateTime? lastUsed;
    }
}
