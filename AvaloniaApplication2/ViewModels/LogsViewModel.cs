using AvaloniaApplication2.Models;
using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaApplication2.ViewModels
{
    public partial class LogsViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;

        public ObservableCollection<NotificationLogEntry> LogEntries { get; } = new();

        [ObservableProperty]
        private string statusMessage = "";

        public LogsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;

            foreach (var entry in settingsService.Settings.NotificationLog.Take(100))
                LogEntries.Add(entry);
        }

        [RelayCommand]
        private async Task ClearLogs()
        {
            LogEntries.Clear();
            await _settingsService.ClearNotificationLogAsync();
            StatusMessage = "日志已清除";
        }

        [RelayCommand]
        private void Refresh()
        {
            LogEntries.Clear();
            foreach (var entry in _settingsService.Settings.NotificationLog.Take(100))
                LogEntries.Add(entry);
            StatusMessage = "日志已刷新";
        }
    }
}
