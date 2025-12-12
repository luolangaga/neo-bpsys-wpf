﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using neo_bpsys_wpf.Core.Abstractions.ViewModels;
using neo_bpsys_wpf.Extensions;
using neo_bpsys_wpf.Core.Abstractions.Extensions;
using CommunityToolkit.Mvvm.Input;
using neo_bpsys_wpf.Core.Abstractions.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace neo_bpsys_wpf.ViewModels.Pages;

public partial class ExtensionPageViewModel : ViewModelBase
{
    private readonly PluginManager _pluginManager;
    private readonly ISettingsHostService _settingsHostService;
    private readonly IPluginMarketplaceService _marketplace;
    private readonly IMessageBoxService _messageBoxService;
    private readonly IAppRestartService _appRestartService;
    [ObservableProperty] private IReadOnlyList<RemotePluginInfo> _remotePlugins = Array.Empty<RemotePluginInfo>();
    [ObservableProperty] private bool _isDownloading;
    [ObservableProperty] private double _downloadPercent;
    [ObservableProperty] private bool _isIndeterminateProgress;

    public ExtensionPageViewModel(PluginManager pluginManager, ISettingsHostService settingsHostService, IPluginMarketplaceService marketplace, IMessageBoxService messageBoxService, IAppRestartService appRestartService)
    {
        _pluginManager = pluginManager;
        _settingsHostService = settingsHostService;
        _marketplace = marketplace;
        _messageBoxService = messageBoxService;
        _appRestartService = appRestartService;
        _pluginManager.PluginsChanged += (_, _) => OnPropertyChanged(nameof(Plugins));
    }

    public IReadOnlyList<PluginMetadata> Plugins => _pluginManager.Metadatas;

    [RelayCommand]
    private void ReloadPlugins()
    {
        _pluginManager.Reload();
    }

    [RelayCommand]
    private void DisablePlugin(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        var disabled = _settingsHostService.Settings.DisabledPlugins;
        if (!disabled.Contains(id))
        {
            // 先动态卸载插件
            _pluginManager.UnloadPlugin(id);
            
            // 然后添加到禁用列表并保存配置
            disabled.Add(id);
            _settingsHostService.SaveConfig();
        }
    }

    [RelayCommand]
    private void EnablePlugin(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        var disabled = _settingsHostService.Settings.DisabledPlugins;
        if (disabled.Remove(id))
        {
            // 先从禁用列表移除并保存配置
            _settingsHostService.SaveConfig();
            
            // 然后动态加载插件
            _pluginManager.LoadPlugin(id);
        }
    }

    [RelayCommand]
    private async Task RefreshMarketplace()
    {
        RemotePlugins = await _marketplace.ListAsync();
    }

    [RelayCommand]
    private async Task InstallPlugin(RemotePluginInfo plugin)
    {
        try
        {
            IsDownloading = true;
            DownloadPercent = 0;
            IsIndeterminateProgress = false;
            var progress = new Progress<double>(p =>
            {
                if (p < 0)
                {
                    IsIndeterminateProgress = true;
                }
                else
                {
                    IsIndeterminateProgress = false;
                    DownloadPercent = Math.Round(p * 100, 1);
                }
            });
            var r = await _marketplace.DownloadAsync(plugin, progress);
            if (!r.downloaded)
            {
                await _messageBoxService.ShowErrorAsync(r.message ?? "下载失败");
                return;
            }
            if (r.requiresRestart)
            {
                var restart = await _messageBoxService.ShowRestartConfirmAsync("插件已安装", "插件已安装，需重启后生效，是否立即重启？");
                if (restart)
                {
                    _appRestartService.RestartApplication();
                }
            }
            else
            {
                _pluginManager.Reload();
                await _messageBoxService.ShowInfoAsync("插件已安装并加载");
            }
        }
        catch (Exception ex)
        {
            await _messageBoxService.ShowErrorAsync($"安装失败: {ex.Message}");
        }
        finally
        {
            IsDownloading = false;
            IsIndeterminateProgress = false;
        }
    }
}
