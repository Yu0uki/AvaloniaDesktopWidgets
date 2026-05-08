using Avalonia.Threading;
using AvaloniaApplication2.ViewModels;
using System;
using System.Collections.ObjectModel;

namespace AvaloniaApplication2.Services
{
    public class NotificationService
    {
        private static NotificationService? _instance;
        public static NotificationService Instance => _instance ??= new NotificationService();

        private readonly ObservableCollection<NotificationMessage> _notifications = new();

        public ObservableCollection<NotificationMessage> Notifications => _notifications;

        public void AddNotification(string message, NotificationType type = NotificationType.Info)
        {
            var notification = new NotificationMessage(message, type);

            void DoAdd()
            {
                _notifications.Add(notification);
                if (_notifications.Count > 10)
                    _notifications.RemoveAt(0);

                NotificationAdded?.Invoke(this, notification);

                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    _notifications.Remove(notification);
                };
                timer.Start();
            }

            if (Dispatcher.UIThread.CheckAccess())
                DoAdd();
            else
                Dispatcher.UIThread.Post(DoAdd);
        }

        public void ClearNotifications()
        {
            _notifications.Clear();
        }

        public event EventHandler<NotificationMessage>? NotificationAdded;

        public void ShowInfo(string message) => AddNotification(message, NotificationType.Info);
        public void ShowSuccess(string message) => AddNotification(message, NotificationType.Success);
        public void ShowWarning(string message) => AddNotification(message, NotificationType.Warning);
        public void ShowError(string message) => AddNotification(message, NotificationType.Error);
    }
}
