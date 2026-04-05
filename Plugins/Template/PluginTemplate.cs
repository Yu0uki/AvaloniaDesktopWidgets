using AvaloniaApplication2.Core;
using Avalonia.Controls;
using System.IO;

namespace PluginTemplate
{
    /// <summary>
    /// 插件模板 - 复制此文件作为新插件的起点
    /// </summary>
    public class PluginTemplate : IPlugin
    {
        // ===== 插件元数据 =====
        
        public string Id => "PluginTemplate";
        public string Name => "插件模板";
        public string Version => "1.0.0";
        public string Description => "这是一个插件模板，用于快速创建新插件";
        public string Author => "Your Name";

        // ===== 生命周期方法 =====

        /// <summary>
        /// 初始化插件（加载时调用一次）
        /// 用途：加载配置、初始化资源、建立连接等
        /// </summary>
        public void Initialize()
        {
            // TODO: 添加初始化逻辑
        }

        /// <summary>
        /// 激活插件（切换到插件视图时调用）
        /// 用途：启动定时器、开始数据更新、订阅事件等
        /// </summary>
        public void Activate()
        {
            // TODO: 添加激活逻辑
        }

        /// <summary>
        /// 停用插件（切换出插件视图时调用）
        /// 用途：暂停定时器、停止数据更新、取消订阅等
        /// </summary>
        public void Deactivate()
        {
            // TODO: 添加停用逻辑
        }

        /// <summary>
        /// 关闭插件（卸载时调用）
        /// 用途：释放资源、保存状态、断开连接等
        /// </summary>
        public void Shutdown()
        {
            // TODO: 添加清理逻辑
        }

        // ===== 视图方法 =====

        /// <summary>
        /// 获取插件主界面
        /// ⚠️ 重要：每次都创建新实例，避免 Visual Tree 冲突
        /// </summary>
        public Control GetMainView()
        {
            return new MainView();
        }

        /// <summary>
        /// 获取插件设置界面（可选）
        /// 返回 null 表示不支持设置
        /// ⚠️ 重要：每次都创建新实例，避免 Visual Tree 冲突
        /// </summary>
        public Control? GetSettingsView()
        {
            // 如果不需要设置界面，返回 null
            // return null;
            
            // 如果需要设置界面，返回新实例
            return new SettingsView();
        }

        /// <summary>
        /// 获取插件图标（可选）
        /// 返回 null 表示使用默认图标
        /// </summary>
        public Stream? GetIcon()
        {
            // 可以返回嵌入资源的图标流
            // var assembly = Assembly.GetExecutingAssembly();
            // return assembly.GetManifestResourceStream("PluginTemplate.Resources.icon.png");
            
            return null;
        }
    }
}
