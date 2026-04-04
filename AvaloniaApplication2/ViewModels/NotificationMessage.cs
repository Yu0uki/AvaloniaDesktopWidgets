using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace AvaloniaApplication2.ViewModels
{
    /// <summary>
    /// 通知消息模型
    /// </summary>
    public partial class NotificationMessage : ObservableObject
    {
        [ObservableProperty]
        private string message = "";

        [ObservableProperty]
        private NotificationType type = NotificationType.Info;

        [ObservableProperty]
        private DateTime timestamp = DateTime.Now;

        // 根据通知类型获取图标
        public string Icon => Type switch
        {
            NotificationType.Success => "✅",
            NotificationType.Warning => "⚠️",
            NotificationType.Error => "❌",
            _ => "ℹ️"
        };

        // 根据通知类型获取颜色
        public string Color => Type switch
        {
            NotificationType.Success => "#107C10",
            NotificationType.Warning => "#FFB900",
            NotificationType.Error => "#D13438",
            _ => "#0078D4"
        };

        public NotificationMessage(string message, NotificationType type = NotificationType.Info)
        {
            Message = message;
            Type = type;
            Timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// 通知类型枚举
    /// </summary>
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
