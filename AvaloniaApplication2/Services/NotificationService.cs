using AvaloniaApplication2.ViewModels;
using System;
using System.Collections.ObjectModel;

namespace AvaloniaApplication2.Services
{
    /// <summary>
    /// 通知服务 - 管理应用程序的全局通知
    /// </summary>
    public class NotificationService
    {
        private static NotificationService? _instance;
        public static NotificationService Instance => _instance ??= new NotificationService();

        private readonly ObservableCollection<NotificationMessage> _notifications = new();
        
        public ObservableCollection<NotificationMessage> Notifications => _notifications;

        /// <summary>
        /// 添加通知
        /// </summary>
        public void AddNotification(string message, NotificationType type = NotificationType.Info)
        {
            var notification = new NotificationMessage(message, type);
            _notifications.Add(notification);

            // 自动移除5秒前的通知（最多保留10条）
            if (_notifications.Count > 10)
            {
                _notifications.RemoveAt(0);
            }

            // 触发通知事件（可用于显示Toast等）
            NotificationAdded?.Invoke(this, notification);
        }

        /// <summary>
        /// 清除所有通知
        /// </summary>
        public void ClearNotifications()
        {
            _notifications.Clear();
        }

        /// <summary>
        /// 通知添加事件
        /// </summary>
        public event EventHandler<NotificationMessage>? NotificationAdded;

        #region 便捷方法

        public void ShowInfo(string message) => AddNotification(message, NotificationType.Info);
        public void ShowSuccess(string message) => AddNotification(message, NotificationType.Success);
        public void ShowWarning(string message) => AddNotification(message, NotificationType.Warning);
        public void ShowError(string message) => AddNotification(message, NotificationType.Error);

        #endregion
    }
}
