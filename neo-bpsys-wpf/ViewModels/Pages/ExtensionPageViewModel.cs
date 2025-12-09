﻿﻿﻿﻿﻿﻿using neo_bpsys_wpf.Core.Abstractions.ViewModels;
using neo_bpsys_wpf.Extensions;
using neo_bpsys_wpf.Core.Abstractions.Extensions;
using CommunityToolkit.Mvvm.Input;
using neo_bpsys_wpf.Core.Abstractions.Services;

namespace neo_bpsys_wpf.ViewModels.Pages;

public partial class ExtensionPageViewModel : ViewModelBase
{
    private readonly PluginManager _pluginManager;
    private readonly ISettingsHostService _settingsHostService;
    public ExtensionPageViewModel(PluginManager pluginManager, ISettingsHostService settingsHostService)
    {
        _pluginManager = pluginManager;
        _settingsHostService = settingsHostService;
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
            disabled.Add(id);
            _settingsHostService.SaveConfig();
            _pluginManager.Reload();
        }
    }

    [RelayCommand]
    private void EnablePlugin(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        var disabled = _settingsHostService.Settings.DisabledPlugins;
        if (disabled.Remove(id))
        {
            _settingsHostService.SaveConfig();
            _pluginManager.Reload();
        }
    }
}
