using AvaloniaApplication2.Models;
using AvaloniaApplication2.Services;
using AvaloniaApplication2.DependencyInjection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvaloniaApplication2.ViewModels
{
    public partial class PluginMarketViewModel : ViewModelBase
    {
        private readonly PluginManager _pluginManager;
        private readonly SettingsService _settingsService;
        private readonly HttpClient _httpClient = new();
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public ObservableCollection<RemotePluginInfo> AllPlugins { get; } = new();

        [ObservableProperty]
        private string searchQuery = "";

        [ObservableProperty]
        private int sortOptionIndex; // 0=最新, 1=名称A-Z

        [ObservableProperty]
        private bool showInstalledOnly;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string statusMessage = "点击刷新加载插件市场";

        [ObservableProperty]
        private string marketUrl = "";

        public string[] SortOptions { get; } = { "最新", "名称 A-Z" };

        public PluginMarketViewModel(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
            _settingsService = ServiceContainer.GetRequiredService<SettingsService>();
            MarketUrl = _settingsService.Settings.PluginMarketUrl;
            _ = LoadMarketAsync();
        }

        partial void OnSearchQueryChanged(string value) => UpdateFilteredView();
        partial void OnSortOptionIndexChanged(int value) => UpdateFilteredView();
        partial void OnShowInstalledOnlyChanged(bool value) => UpdateFilteredView();

        /// <summary>
        /// 从 GitHub 插件市场索引加载
        /// </summary>
        [RelayCommand]
        private async Task LoadMarketAsync()
        {
            IsLoading = true;
            StatusMessage = "正在连接插件市场...";

            try
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("EfficiencyWorkshop/4.1");
                _httpClient.Timeout = TimeSpan.FromSeconds(15);

                var json = await _httpClient.GetStringAsync(MarketUrl);
                var index = JsonSerializer.Deserialize<PluginMarketIndex>(json, JsonOpts);

                if (index?.plugins == null || index.plugins.Length == 0)
                {
                    StatusMessage = "插件市场暂无内容";
                    AllPlugins.Clear();
                    UpdateFilteredView();
                    return;
                }

                AllPlugins.Clear();
                foreach (var entry in index.plugins)
                {
                    DateTime.TryParse(entry.updatedAt, out var updated);
                    if (updated == default) updated = DateTime.Now.AddDays(-7);

                    // 已安装插件优先使用实时版本号
                    var installedInfo = _pluginManager.PluginInfos.FirstOrDefault(p =>
                        string.Equals(p.Id, entry.id, StringComparison.OrdinalIgnoreCase));
                    var installed = installedInfo != null;
                    var liveVersion = installed ? installedInfo!.Version : entry.version;

                    AllPlugins.Add(new RemotePluginInfo
                    {
                        Id = entry.id,
                        Name = entry.name,
                        Author = entry.author,
                        Description = entry.description,
                        Version = liveVersion,
                        UpdatedAt = updated,
                        DownloadUrl = entry.downloadUrl,
                        Icon = string.IsNullOrEmpty(entry.icon) ? "📦" : entry.icon,
                        Tags = entry.tags ?? Array.Empty<string>(),
                        IsInstalled = installed,
                        InstallStatus = installed ? "已安装" : "安装"
                    });
                }

                UpdateFilteredView();
                StatusMessage = $"已加载 {AllPlugins.Count} 个插件";
            }
            catch (HttpRequestException ex)
            {
                var detail = ex.StatusCode.HasValue
                    ? $" (HTTP {(int)ex.StatusCode})" : "";
                StatusMessage = $"无法连接远程市场{detail}，尝试加载本地缓存...";
                await TryLoadLocalIndexAsync();
            }
            catch (TaskCanceledException)
            {
                StatusMessage = "连接超时，尝试加载本地缓存...";
                await TryLoadLocalIndexAsync();
            }
            catch (JsonException)
            {
                StatusMessage = "市场数据格式错误，尝试加载本地缓存...";
                await TryLoadLocalIndexAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载失败: {ex.Message}，尝试加载本地缓存...";
                await TryLoadLocalIndexAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 本地回退：优先从已安装的插件中生成实时列表，合并本地 index.json 元数据
        /// </summary>
        private async Task TryLoadLocalIndexAsync()
        {
            try
            {
                // 加载本地 index.json 作为元数据源
                Dictionary<string, PluginMarketEntry>? metaLookup = null;
                var localPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(
                    System.AppContext.BaseDirectory, "..", "..", "..", "..", "Plugins", "index.json"));

                if (System.IO.File.Exists(localPath))
                {
                    try
                    {
                        var json = await System.IO.File.ReadAllTextAsync(localPath);
                        var index = JsonSerializer.Deserialize<PluginMarketIndex>(json, JsonOpts);
                        if (index?.plugins != null)
                            metaLookup = index.plugins.ToDictionary(e => e.id, StringComparer.OrdinalIgnoreCase);
                    }
                    catch { /* 元数据损坏时忽略 */ }
                }

                AllPlugins.Clear();

                // 从已安装插件生成实时条目（版本号始终准确）
                foreach (var pi in _pluginManager.PluginInfos.Where(p => p.IsLoaded))
                {
                    var meta = metaLookup?.GetValueOrDefault(pi.Id);
                    AllPlugins.Add(new RemotePluginInfo
                    {
                        Id = pi.Id,
                        Name = meta?.name ?? pi.Name,
                        Author = meta?.author ?? "Efficiency Workshop Team",
                        Description = meta?.description ?? pi.Description ?? "",
                        Version = pi.Version, // 实时版本，非缓存
                        UpdatedAt = DateTime.Now.AddDays(-1),
                        DownloadUrl = meta?.downloadUrl ?? "",
                        Icon = meta?.icon ?? "📦",
                        Tags = meta?.tags ?? Array.Empty<string>(),
                        IsInstalled = true,
                        InstallStatus = "已安装"
                    });
                }

                // 补充本地元数据中但未安装的插件
                if (metaLookup is { Count: > 0 })
                {
                    foreach (var kvp in metaLookup)
                    {
                        var id = kvp.Key;
                        var entry = kvp.Value;
                        if (_pluginManager.PluginInfos.Any(p =>
                            string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase)))
                            continue;

                        DateTime.TryParse(entry.updatedAt, out var updated);
                        if (updated == default) updated = DateTime.Now.AddDays(-7);

                        AllPlugins.Add(new RemotePluginInfo
                        {
                            Id = entry.id,
                            Name = entry.name,
                            Author = entry.author,
                            Description = entry.description,
                            Version = entry.version,
                            UpdatedAt = updated,
                            DownloadUrl = entry.downloadUrl,
                            Icon = string.IsNullOrEmpty(entry.icon) ? "📦" : entry.icon,
                            Tags = entry.tags ?? Array.Empty<string>(),
                            IsInstalled = false,
                            InstallStatus = "安装"
                        });
                    }
                }

                UpdateFilteredView();
                StatusMessage = $"本地模式: {AllPlugins.Count} 个插件 ({_pluginManager.PluginInfos.Count(p => p.IsLoaded)} 个已安装)";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载本地缓存失败: {ex.Message}";
            }
        }

        private readonly ObservableCollection<RemotePluginInfo> _filteredPlugins = new();
        public ObservableCollection<RemotePluginInfo> FilteredPlugins => _filteredPlugins;

        private void UpdateFilteredView()
        {
            var query = (SearchQuery ?? "").Trim().ToLowerInvariant();
            var filtered = AllPlugins.AsEnumerable();

            // 搜索过滤
            if (!string.IsNullOrEmpty(query))
                filtered = filtered.Where(p =>
                    p.Name.ToLowerInvariant().Contains(query) ||
                    p.Description.ToLowerInvariant().Contains(query) ||
                    p.Author.ToLowerInvariant().Contains(query) ||
                    p.Tags.Any(t => t.ToLowerInvariant().Contains(query)));

            // 仅已安装
            if (ShowInstalledOnly)
                filtered = filtered.Where(p => p.IsInstalled);

            // 排序
            filtered = SortOptionIndex switch
            {
                1 => filtered.OrderBy(p => p.Name.ToLowerInvariant()),     // 名称 A-Z
                _ => filtered.OrderByDescending(p => p.UpdatedAt)          // 最新
            };

            _filteredPlugins.Clear();
            foreach (var p in filtered)
                _filteredPlugins.Add(p);
        }

        /// <summary>
        /// 安装/更新插件：从远程URL下载DLL
        /// </summary>
        [RelayCommand]
        private async Task InstallPluginAsync(RemotePluginInfo? plugin)
        {
            if (plugin == null || plugin.IsInstalling) return;
            if (plugin.IsInstalled)
            {
                StatusMessage = $"{plugin.Name} 已安装";
                return;
            }

            var notify = NotificationService.Instance;
            notify.ShowInfo($"开始下载插件: {plugin.Name} v{plugin.Version}");

            plugin.IsInstalling = true;
            plugin.InstallStatus = "正在下载...";

            try
            {
                byte[]? dllBytes = null;
                try
                {
                    dllBytes = await _httpClient.GetByteArrayAsync(plugin.DownloadUrl);
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // 尝试替换 Plugin.dll ↔ .dll 的命名差异
                    var altUrl = plugin.DownloadUrl.Contains("Plugin.dll")
                        ? plugin.DownloadUrl.Replace("Plugin.dll", ".dll")
                        : plugin.DownloadUrl.Replace(".dll", "Plugin.dll");
                    if (altUrl != plugin.DownloadUrl)
                    {
                        StatusMessage = $"尝试备用链接...";
                        dllBytes = await _httpClient.GetByteArrayAsync(altUrl);
                    }
                    else throw;
                }

                if (dllBytes == null) return;
                notify.ShowInfo($"{plugin.Name} 下载完成 ({dllBytes.Length / 1024}KB)，正在安装...");

                var pluginsDir = _pluginManager.PluginsDirectory;
                var dllPath = System.IO.Path.Combine(pluginsDir, $"{plugin.Id}.dll");

                var tmpPath = dllPath + ".tmp";
                await System.IO.File.WriteAllBytesAsync(tmpPath, dllBytes);
                System.IO.File.Move(tmpPath, dllPath, overwrite: true);

                await _pluginManager.LoadPluginAsync(dllPath);

                plugin.IsInstalled = true;
                plugin.InstallStatus = "已安装";
                StatusMessage = $"{plugin.Name} 安装成功";
                notify.ShowSuccess($"插件安装成功: {plugin.Name} v{plugin.Version}");

                // 自动在侧边栏中激活插件
                var mainVM = DependencyInjection.ServiceContainer.GetService<MainWindowViewModel>();
                if (mainVM != null)
                {
                    _pluginManager.StartPlugin(plugin.Id);
                    _pluginManager.ActivatePlugin(plugin.Id);
                    mainVM.NavigateToPluginCommand.Execute(plugin.Id);
                }

                UpdateFilteredView();
            }
            catch (Exception ex)
            {
                plugin.InstallStatus = "安装失败";
                StatusMessage = $"安装失败: {ex.Message}";
                notify.ShowError($"插件安装失败: {plugin.Name} — {ex.Message}");
            }
            finally
            {
                plugin.IsInstalling = false;
            }
        }

        /// <summary>
        /// 更新市场 URL 并重新加载
        /// </summary>
        public async Task UpdateMarketUrlAsync(string url)
        {
            MarketUrl = url;
            _settingsService.Settings.PluginMarketUrl = url;
            await _settingsService.SaveSettingsAsync();
            await LoadMarketAsync();
        }

        [RelayCommand]
        private void Refresh()
        {
            _ = LoadMarketAsync();
        }
    }
}
