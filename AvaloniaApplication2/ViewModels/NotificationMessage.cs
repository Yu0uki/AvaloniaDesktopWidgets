using AvaloniaApplication2.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace AvaloniaApplication2.ViewModels
{
    public partial class NotificationMessage : ObservableObject
    {
        [ObservableProperty]
        private string message = "";

        [ObservableProperty]
        private NotificationType type = NotificationType.Info;

        [ObservableProperty]
        private DateTime timestamp = DateTime.Now;

        public string Icon => Type switch
        {
            NotificationType.Success => FluentIcons.CheckmarkCircle,
            NotificationType.Warning => FluentIcons.Warning,
            NotificationType.Error => FluentIcons.ErrorCircle,
            _ => FluentIcons.InfoCircle
        };

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

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
