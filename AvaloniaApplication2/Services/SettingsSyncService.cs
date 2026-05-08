using AvaloniaApplication2.Models;
using AvaloniaApplication2.Infrastructure;
using Serilog;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvaloniaApplication2.Services
{
    /// <summary>
    /// 设置同步服务 — 将本地设置文件推送到 GitHub 仓库
    /// 支持私有仓库（Personal Access Token 认证）
    /// </summary>
    public class SettingsSyncService
    {
        private readonly SettingsService _settingsService;
        private readonly NotificationService _notify;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

        public SettingsSyncService(SettingsService settingsService, NotificationService notificationService)
        {
            _settingsService = settingsService;
            _notify = notificationService;
            _httpClient = new HttpClient();
            _logger = LoggingConfig.Logger.ForContext<SettingsSyncService>();
        }

        /// <summary>
        /// 同步设置文件到 GitHub
        /// </summary>
        public async Task<string> SyncAsync()
        {
            var s = _settingsService.Settings;
            if (!s.SyncEnabled) return "同步功能未启用";
            if (string.IsNullOrWhiteSpace(s.SyncRepoOwner) || string.IsNullOrWhiteSpace(s.SyncRepoName))
                return "请先配置 GitHub 仓库信息";
            if (string.IsNullOrWhiteSpace(s.SyncToken))
                return "请先配置 GitHub 访问令牌 (PAT)";

            _notify.ShowInfo("开始同步设置到 GitHub...");
            try
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("EfficiencyWorkshop/4.1");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", s.SyncToken);
                _httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

                // 同步 settings.json
                var dataDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                var settingsPath = System.IO.Path.Combine(dataDir, "settings.json");
                var dashPath = System.IO.Path.Combine(dataDir, "dashboard_layout.json");

                var results = new System.Collections.Generic.List<string>();

                if (System.IO.File.Exists(settingsPath))
                {
                    var content = await System.IO.File.ReadAllTextAsync(settingsPath);
                    var r = await PushFileAsync(s, "settings.json", content);
                    results.Add($"settings.json: {r}");
                }

                if (System.IO.File.Exists(dashPath))
                {
                    var content = await System.IO.File.ReadAllTextAsync(dashPath);
                    var r = await PushFileAsync(s, "dashboard_layout.json", content);
                    results.Add($"dashboard: {r}");
                }

                // 更新最后同步时间
                s.LastSyncTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await _settingsService.SaveSettingsAsync();

                _logger.Information("设置同步完成");
                var result = "同步成功 (" + string.Join(", ", results) + ")";
                _notify.ShowSuccess($"设置同步完成 — {s.SyncRepoOwner}/{s.SyncRepoName}");
                return result;
            }
            catch (HttpRequestException ex)
            {
                var msg = ex.Message.Contains("401") ? "认证失败，请检查 Token" :
                          ex.Message.Contains("404") ? "仓库不存在，请检查仓库名称" :
                          ex.Message.Contains("403") ? "权限不足，请检查 Token 权限" :
                          $"网络错误: {ex.Message}";
                _logger.Error(ex, "设置同步失败");
                _notify.ShowError($"设置同步失败: {msg}");
                return msg;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "设置同步异常");
                _notify.ShowError($"设置同步失败: {ex.Message}");
                return $"同步失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 推送单个文件到 GitHub（PUT /repos/{owner}/{repo}/contents/{path}）
        /// </summary>
        private async Task<string> PushFileAsync(AppSettings s, string fileName, string content)
        {
            var apiUrl = $"https://api.github.com/repos/{s.SyncRepoOwner}/{s.SyncRepoName}/contents/{fileName}";

            // 先获取文件 SHA（如果已存在则需要更新）
            string? sha = null;
            try
            {
                var getResp = await _httpClient.GetAsync(apiUrl + $"?ref={s.SyncBranch}");
                if (getResp.IsSuccessStatusCode)
                {
                    var getJson = await getResp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(getJson);
                    sha = doc.RootElement.GetProperty("sha").GetString();
                }
            }
            catch { /* 文件不存在，将创建新文件 */ }

            // Base64 编码内容
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
            var body = new
            {
                message = $"sync: update {fileName} [{DateTime.Now:yyyy-MM-dd HH:mm:ss}]",
                content = base64,
                branch = s.SyncBranch,
                sha
            };

            var bodyJson = JsonSerializer.Serialize(body, JsonOpts);
            var httpContent = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            var resp = await _httpClient.PutAsync(apiUrl, httpContent);
            if (resp.IsSuccessStatusCode)
                return "OK";
            else
            {
                var err = await resp.Content.ReadAsStringAsync();
                return $"HTTP {(int)resp.StatusCode}";
            }
        }
    }
}
