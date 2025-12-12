using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using neo_bpsys_wpf.Core.Abstractions.Extensions;
using neo_bpsys_wpf.Core.Abstractions.Services;
using neo_bpsys_wpf.Core.Abstractions.ViewModels;
using neo_bpsys_wpf.Core.Enums;
using neo_bpsys_wpf.Core.Models;
using neo_bpsys_wpf.Extensions;
using System.Collections.ObjectModel;

namespace neo_bpsys_wpf.ViewModels.Pages;

/// <summary>
/// 插件控件管理页面 ViewModel
/// </summary>
public partial class PluginControlManagePageViewModel : ViewModelBase
{
    private readonly PluginManager _pluginManager;
    private readonly ISettingsHostService _settingsHostService;
    private readonly IFrontService _frontService;
    private readonly IMessageBoxService _messageBoxService;

    [ObservableProperty] 
    private ObservableCollection<PluginControlDisplayInfo> _controls = new();

    [ObservableProperty] 
    private int _totalControlCount;

    [ObservableProperty] 
    private int _visibleControlCount;

    [ObservableProperty] 
    private int _hiddenControlCount;

    [ObservableProperty] 
    private string _searchText = string.Empty;

    public PluginControlManagePageViewModel(
        PluginManager pluginManager,
        ISettingsHostService settingsHostService,
        IFrontService frontService,
        IMessageBoxService messageBoxService)
    {
        _pluginManager = pluginManager;
        _settingsHostService = settingsHostService;
        _frontService = frontService;
        _messageBoxService = messageBoxService;

        // 监听插件变化
        _pluginManager.PluginsChanged += (_, _) => RefreshControls();
    }

    /// <summary>
    /// 刷新控件列表
    /// </summary>
    [RelayCommand]
    private void RefreshControls()
    {
        Controls.Clear();

        var config = _settingsHostService.Settings.PluginControlDisplayConfig;
        var pluginMetadataDict = _pluginManager.Metadatas.ToDictionary(m => m.Id, m => m);

        foreach (var descriptor in _pluginManager.OverlayDescriptors)
        {
            // 从描述符ID中提取插件ID (假设格式为 "PluginId.ControlId")
            var pluginId = ExtractPluginId(descriptor.Id);
            var pluginName = pluginMetadataDict.TryGetValue(pluginId, out var metadata) 
                ? metadata.Name 
                : pluginId;

            var windowName = GetWindowTypeName(descriptor.TargetWindowType);

            // 从配置中读取显示状态,默认为 true
            var isVisible = config.ControlVisibility.TryGetValue(descriptor.Id, out var visible) 
                ? visible 
                : true;

            var controlInfo = new PluginControlDisplayInfo
            {
                ControlId = descriptor.Id,
                DisplayName = descriptor.DisplayName,
                PluginId = pluginId,
                PluginName = pluginName,
                TargetWindowType = descriptor.TargetWindowType,
                TargetWindowName = windowName,
                CanvasName = descriptor.CanvasName,
                IsVisible = isVisible
            };

            // 监听可见性变化
            controlInfo.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PluginControlDisplayInfo.IsVisible) && s is PluginControlDisplayInfo info)
                {
                    OnControlVisibilityChanged(info);
                }
            };

            Controls.Add(controlInfo);
        }

        UpdateStatistics();
    }

    /// <summary>
    /// 从控件ID中提取插件ID
    /// </summary>
    private string ExtractPluginId(string controlId)
    {
        // 假设控件ID格式为 "PluginId.ControlName" 或 "PluginId_ControlName"
        var separators = new[] { '.', '_' };
        foreach (var sep in separators)
        {
            var index = controlId.IndexOf(sep);
            if (index > 0)
            {
                return controlId.Substring(0, index);
            }
        }
        return controlId;
    }

    /// <summary>
    /// 获取窗口类型的显示名称
    /// </summary>
    private string GetWindowTypeName(FrontWindowType windowType)
    {
        return windowType switch
        {
            FrontWindowType.BpWindow => "BP窗口",
            FrontWindowType.CutSceneWindow => "过场动画窗口",
            FrontWindowType.ScoreWindow => "比分窗口",
            FrontWindowType.ScoreSurWindow => "求生者比分窗口",
            FrontWindowType.ScoreHunWindow => "监管者比分窗口",
            FrontWindowType.ScoreGlobalWindow => "全局比分窗口",
            FrontWindowType.GameDataWindow => "游戏数据窗口",
            FrontWindowType.WidgetsWindow => "小部件窗口",
            FrontWindowType.PluginOverlayWindow => "插件覆盖窗口",
            _ => windowType.ToString()
        };
    }

    /// <summary>
    /// 控件可见性变化时触发
    /// </summary>
    private void OnControlVisibilityChanged(PluginControlDisplayInfo info)
    {
        // 更新配置
        var config = _settingsHostService.Settings.PluginControlDisplayConfig;
        config.ControlVisibility[info.ControlId] = info.IsVisible;
        _settingsHostService.SaveConfig();

        // 通知前台服务更新控件显示状态
        if (info.IsVisible)
        {
            // 找到对应的描述符并重新添加控件
            var descriptor = _pluginManager.OverlayDescriptors
                .FirstOrDefault(d => d.Id == info.ControlId);
            if (descriptor != null)
            {
                _frontService.AddPluginOverlayControl(descriptor);
            }
        }
        else
        {
            // 移除控件
            _frontService.RemovePluginOverlayControl(info.ControlId);
        }

        UpdateStatistics();
    }

    /// <summary>
    /// 更新统计信息
    /// </summary>
    private void UpdateStatistics()
    {
        TotalControlCount = Controls.Count;
        VisibleControlCount = Controls.Count(c => c.IsVisible);
        HiddenControlCount = TotalControlCount - VisibleControlCount;
    }

    /// <summary>
    /// 显示所有控件
    /// </summary>
    [RelayCommand]
    private void ShowAllControls()
    {
        foreach (var control in Controls)
        {
            control.IsVisible = true;
        }
    }

    /// <summary>
    /// 隐藏所有控件
    /// </summary>
    [RelayCommand]
    private void HideAllControls()
    {
        foreach (var control in Controls)
        {
            control.IsVisible = false;
        }
    }

    /// <summary>
    /// 应用搜索过滤
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        // TODO: 实现搜索过滤功能
        // 这里可以使用 CollectionViewSource 或其他方式实现过滤
    }

    /// <summary>
    /// 重置为默认显示
    /// </summary>
    [RelayCommand]
    private async Task ResetToDefaultAsync()
    {
        var result = await _messageBoxService.ShowConfirmAsync(
            "确认重置",
            "确定要重置所有控件的显示状态为默认吗?",
            "确认",
            "取消");

        if (result)
        {
            foreach (var control in Controls)
            {
                control.IsVisible = true;
            }
        }
    }
}
