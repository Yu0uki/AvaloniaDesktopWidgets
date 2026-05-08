using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PasswordGeneratorPlugin.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty] private int passwordLength = 16;
        [ObservableProperty] private bool useUppercase = true;
        [ObservableProperty] private bool useLowercase = true;
        [ObservableProperty] private bool useNumbers = true;
        [ObservableProperty] private bool useSymbols = true;
        [ObservableProperty] private string generatedPassword = "";
        [ObservableProperty] private string strengthText = "";
        [ObservableProperty] private string strengthColor = "#888888";

        private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string NumberChars = "0123456789";
        private const string SymbolChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        partial void OnPasswordLengthChanged(int value) => UpdateStrengthHint();
        partial void OnUseUppercaseChanged(bool value) => UpdateStrengthHint();
        partial void OnUseLowercaseChanged(bool value) => UpdateStrengthHint();
        partial void OnUseNumbersChanged(bool value) => UpdateStrengthHint();
        partial void OnUseSymbolsChanged(bool value) => UpdateStrengthHint();

        private void UpdateStrengthHint()
        {
            var pool = (UseUppercase ? 26 : 0) + (UseLowercase ? 26 : 0)
                     + (UseNumbers ? 10 : 0) + (UseSymbols ? 20 : 0);
            if (pool == 0) { StrengthText = "请至少选择一种字符"; StrengthColor = "#D13438"; return; }
            var bits = Math.Log2(pool) * PasswordLength;
            (StrengthText, StrengthColor) = bits switch
            {
                < 40 => ("强度: 弱", "#D13438"),
                < 60 => ("强度: 一般", "#FFB900"),
                < 80 => ("强度: 强", "#10893E"),
                _ => ("强度: 非常强", "#0078D4")
            };
        }

        [RelayCommand]
        public void Generate()
        {
            var pool = "";
            if (UseUppercase) pool += UppercaseChars;
            if (UseLowercase) pool += LowercaseChars;
            if (UseNumbers) pool += NumberChars;
            if (UseSymbols) pool += SymbolChars;
            if (pool.Length == 0) { GeneratedPassword = ""; return; }

            var chars = new char[PasswordLength];
            var bytes = RandomNumberGenerator.GetBytes(PasswordLength * 4);
            for (int i = 0; i < PasswordLength; i++)
            {
                var idx = BitConverter.ToUInt32(bytes, i * 4) % (uint)pool.Length;
                chars[i] = pool[(int)idx];
            }
            GeneratedPassword = new string(chars);
            UpdateStrengthHint();
        }

        [RelayCommand]
        public async Task CopyToClipboard()
        {
            if (string.IsNullOrEmpty(GeneratedPassword)) return;
            if (Avalonia.Application.Current?.ApplicationLifetime
                is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow?.Clipboard is { } clipboard)
            {
                await clipboard.SetTextAsync(GeneratedPassword);
            }
        }
    }
}
