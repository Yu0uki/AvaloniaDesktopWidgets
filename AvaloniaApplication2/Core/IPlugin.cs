using Avalonia.Controls;
using System.IO;

namespace AvaloniaApplication2.Core
{
    /// <summary>
    /// 插件接口 - 所有插件必须实现此接口
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 插件唯一标识符
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 插件显示名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 插件版本号
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 插件描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 插件作者
        /// </summary>
        string Author { get; }

        /// <summary>
        /// 初始化插件（加载时调用一次）
        /// </summary>
        void Initialize();

        /// <summary>
        /// 激活插件（切换到插件视图时调用）
        /// </summary>
        void Activate();

        /// <summary>
        /// 停用插件（切换出插件视图时调用）
        /// </summary>
        void Deactivate();

        /// <summary>
        /// 关闭插件（卸载时调用）
        /// </summary>
        void Shutdown();

        /// <summary>
        /// 获取插件主界面
        /// </summary>
        Control GetMainView();

        /// <summary>
        /// 获取插件设置界面（可选）
        /// </summary>
        Control? GetSettingsView();

        /// <summary>
        /// 获取插件图标（可选）
        /// </summary>
        Stream? GetIcon();
    }
}
