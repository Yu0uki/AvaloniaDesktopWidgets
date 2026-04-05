using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Timers;

namespace WorldClockPlugin.ViewModels
{
    public partial class WorldClockViewModel : ObservableObject, IDisposable
    {
        private Timer? _timer;
        
        [ObservableProperty]
        private DateTime currentTime = DateTime.Now;

        [ObservableProperty]
        private string selectedTimeZone = "Local";

        [ObservableProperty]
        private string currentCity = "本地";

        [ObservableProperty]
        private string currentCountry = "中国";

        [ObservableProperty]
        private string timeFormat = "yyyy-MM-dd HH:mm:ss";

        [ObservableProperty]
        private bool isIndependentWindow = false;

        // 秒表相关
        [ObservableProperty]
        private TimeSpan stopwatchTime = TimeSpan.Zero;

        [ObservableProperty]
        private bool isStopwatchRunning = false;

        // 计时器相关
        [ObservableProperty]
        private TimeSpan timerTime = TimeSpan.Zero;

        [ObservableProperty]
        private TimeSpan timerDuration = TimeSpan.FromMinutes(5);

        [ObservableProperty]
        private bool isTimerRunning = false;

        [ObservableProperty]
        private int activeTab = 0; // 0: 时钟, 1: 秒表, 2: 计时器

        public ObservableCollection<string> TimeZones { get; } = new()
        {
            "Local",
            "UTC",
            "America/New_York",
            "America/Los_Angeles",
            "Europe/London",
            "Europe/Paris",
            "Asia/Tokyo",
            "Asia/Shanghai",
            "Asia/Hong_Kong",
            "Australia/Sydney"
        };

        public WorldClockViewModel()
        {
            StartClock();
        }

        private void StartClock()
        {
            _timer = new Timer(1000); // 每秒更新
            _timer.Elapsed += (s, e) => UpdateTime();
            _timer.Start();
        }

        private void UpdateTime()
        {
            try
            {
                TimeZoneInfo timeZone;
                
                switch (SelectedTimeZone)
                {
                    case "Local":
                        timeZone = TimeZoneInfo.Local;
                        CurrentCity = "本地";
                        CurrentCountry = "中国";
                        break;
                    case "UTC":
                        timeZone = TimeZoneInfo.Utc;
                        CurrentCity = "UTC";
                        CurrentCountry = "协调世界时";
                        break;
                    case "America/New_York":
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                        CurrentCity = "纽约";
                        CurrentCountry = "美国";
                        break;
                    case "America/Los_Angeles":
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                        CurrentCity = "洛杉矶";
                        CurrentCountry = "美国";
                        break;
                    case "Europe/London":
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                        CurrentCity = "伦敦";
                        CurrentCountry = "英国";
                        break;
                    case "Europe/Paris":
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
                        CurrentCity = "巴黎";
                        CurrentCountry = "法国";
                        break;
                    case "Asia/Tokyo":
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                        CurrentCity = "东京";
                        CurrentCountry = "日本";
                        break;
                    case "Asia/Shanghai":
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                        CurrentCity = "上海";
                        CurrentCountry = "中国";
                        break;
                    case "Asia/Hong_Kong":
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                        CurrentCity = "香港";
                        CurrentCountry = "中国";
                        break;
                    case "Australia/Sydney":
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
                        CurrentCity = "悉尼";
                        CurrentCountry = "澳大利亚";
                        break;
                    default:
                        timeZone = TimeZoneInfo.Local;
                        CurrentCity = "本地";
                        CurrentCountry = "中国";
                        break;
                }

                CurrentTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone);
            }
            catch (Exception)
            {
                // 如果时区转换失败，使用本地时间
                CurrentTime = DateTime.Now;
            }
        }

        [RelayCommand]
        private void StartStopwatch()
        {
            if (!IsStopwatchRunning)
            {
                IsStopwatchRunning = true;
                var swTimer = new Timer(10); // 每10毫秒更新
                swTimer.Elapsed += (s, e) =>
                {
                    if (IsStopwatchRunning)
                    {
                        StopwatchTime = StopwatchTime.Add(TimeSpan.FromMilliseconds(10));
                    }
                };
                swTimer.Start();
            }
        }

        [RelayCommand]
        private void StopStopwatch()
        {
            IsStopwatchRunning = false;
        }

        [RelayCommand]
        private void ResetStopwatch()
        {
            IsStopwatchRunning = false;
            StopwatchTime = TimeSpan.Zero;
        }

        [RelayCommand]
        private void StartTimer()
        {
            if (!IsTimerRunning && TimerDuration > TimeSpan.Zero)
            {
                IsTimerRunning = true;
                TimerTime = TimerDuration;
                
                var tTimer = new Timer(1000); // 每秒更新
                tTimer.Elapsed += (s, e) =>
                {
                    if (IsTimerRunning && TimerTime > TimeSpan.Zero)
                    {
                        TimerTime = TimerTime.Subtract(TimeSpan.FromSeconds(1));
                    }
                    else if (TimerTime <= TimeSpan.Zero)
                    {
                        IsTimerRunning = false;
                        tTimer.Stop();
                        // 这里可以添加计时器结束的通知
                    }
                };
                tTimer.Start();
            }
        }

        [RelayCommand]
        private void StopTimer()
        {
            IsTimerRunning = false;
        }

        [RelayCommand]
        private void ResetTimer()
        {
            IsTimerRunning = false;
            TimerTime = TimerDuration;
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}
