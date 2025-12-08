using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using neo_bpsys_wpf.Core.Abstractions.Services;
using neo_bpsys_wpf.Core.Abstractions.ViewModels;

namespace neo_bpsys_wpf.ViewModels.Pages;

public partial class ExtensionPageViewModel : ViewModelBase
{
    private readonly IPluginService _pluginService;
    private readonly IMessageBoxService _messageBoxService;

    public ObservableCollection<PluginItemView> Plugins { get; } = new();

    public ExtensionPageViewModel(IPluginService pluginService, IMessageBoxService messageBoxService)
    {
        _pluginService = pluginService;
        _messageBoxService = messageBoxService;
        LoadPlugins();
    }

    private void LoadPlugins()
    {
        Plugins.Clear();
        foreach (var p in _pluginService.Plugins)
        {
            Plugins.Add(new PluginItemView
            {
                Id = p.Metadata.Id,
                Name = p.Metadata.Name,
                Description = p.Metadata.Description,
                Author = p.Metadata.Author,
                Version = p.Metadata.Version,
                IsEnabled = _pluginService.IsEnabled(p.Metadata.Id)
            });
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        _pluginService.Discover();
        LoadPlugins();
    }

    [RelayCommand]
    private void Toggle(string id)
    {
        var current = _pluginService.IsEnabled(id);
        _pluginService.SetEnabled(id, !current);
        LoadPlugins();
    }
}

public class PluginItemView : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}
