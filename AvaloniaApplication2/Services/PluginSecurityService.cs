using AvaloniaApplication2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AvaloniaApplication2.Services
{
    /// <summary>
    /// 插件安全检测服务 — 加载前对 DLL 进行安全检查（仅模式扫描，不加载程序集）
    /// </summary>
    public class PluginSecurityService
    {
        private readonly NotificationService _notify;

        public bool Enabled { get; set; } = true;

        // 危险 API 模式（字符串匹配，不加载程序集）
        private static readonly string[] DangerousPatterns =
        {
            "Process.Start", "Win32.Registry", "File.Delete",
            "Directory.Delete", "Process.Kill", "Socket.Connect",
            "WebClient.Download", "Assembly.Load",
        };

        public PluginSecurityService(NotificationService notificationService)
        {
            _notify = notificationService;
        }

        /// <summary>
        /// 扫描 DLL（仅读取字节进行模式匹配，不执行任何代码）
        /// </summary>
        public SecurityScanResult ScanDll(string dllPath)
        {
            var result = new SecurityScanResult { FilePath = dllPath, IsSafe = true };
            try
            {
                result.FileName = Path.GetFileName(dllPath);
                result.FileSize = new FileInfo(dllPath).Length;

                using var sha = SHA256.Create();
                using var fs = File.OpenRead(dllPath);
                result.FileHash = Convert.ToHexString(sha.ComputeHash(fs));

                if (!Enabled)
                {
                    result.Warnings.Add("安全检查已禁用");
                    return result;
                }

                fs.Position = 0;
                var bytes = new byte[Math.Min(fs.Length, 1024 * 1024)]; // 读取前1MB
                fs.Read(bytes, 0, bytes.Length);
                var text = Encoding.ASCII.GetString(bytes);

                foreach (var pattern in DangerousPatterns)
                {
                    if (text.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                        result.Warnings.Add($"危险模式: {pattern}");
                }

                if (result.Warnings.Count > 0) result.RiskLevel = "中";
                if (result.Warnings.Count >= 3) result.RiskLevel = "高";
            }
            catch (Exception ex)
            {
                result.IsSafe = false;
                result.Warnings.Add($"扫描异常: {ex.Message}");
            }
            return result;
        }
    }

    public class SecurityScanResult
    {
        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }
        public string FileHash { get; set; } = "";
        public bool IsSafe { get; set; } = true;
        public string RiskLevel { get; set; } = "低";
        public List<string> Warnings { get; set; } = new();
    }
}
