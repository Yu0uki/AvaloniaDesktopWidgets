using AvaloniaApplication2.Core;
using Avalonia.Controls;
using System.IO;

namespace NotepadPlugin
{
    /// <summary>
    /// 记事本插件 - 支持在指定文件夹保存笔记，可调节字号和字体
    /// </summary>
    public class NotepadPlugin : IPlugin
    {
        // ===== 插件元数据 =====
        
        public string Id => "NotepadPlugin";
        public string Name => "记事本";
        public string Version => "1.0.0";
        public string Description => "一个简单的记事本插件，支持在指定文件夹保存笔记，可调节字号和字体";
        public string Author => "AI Assistant";

        // ===== 生命周期方法 =====

        /// <summary>
        /// 初始化插件（加载时调用一次）
        /// </summary>
        public void Initialize()
        {
            // TODO: 添加初始化逻辑，如加载配置
        }

        /// <summary>
        /// 激活插件（切换到插件视图时调用）
        /// </summary>
        public void Activate()
        {
            // TODO: 添加激活逻辑，如启动定时器
        }

        /// <summary>
        /// 停用插件（切换出插件视图时调用）
        /// </summary>
        public void Deactivate()
        {
            // TODO: 添加停用逻辑，如暂停定时器
        }

        /// <summary>
        /// 关闭插件（卸载时调用）
        /// </summary>
        public void Shutdown()
        {
            // TODO: 添加清理逻辑，如保存状态
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
        /// 获取插件设置界面
        /// ⚠️ 重要：每次都创建新实例，避免 Visual Tree 冲突
        /// </summary>
        public Control? GetSettingsView()
        {
            return new SettingsView();
        }

        /// <summary>
        /// 获取插件图标（可选）
        /// </summary>
        public Stream? GetIcon()
        {
            return null;
        }
    }
}
