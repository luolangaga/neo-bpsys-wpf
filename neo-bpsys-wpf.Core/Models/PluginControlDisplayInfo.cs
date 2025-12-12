using CommunityToolkit.Mvvm.ComponentModel;
using neo_bpsys_wpf.Core.Abstractions.ViewModels;
using neo_bpsys_wpf.Core.Enums;

namespace neo_bpsys_wpf.Core.Models;

/// <summary>
/// 插件控件显示信息 - 用于管理插件控件的显示状态
/// </summary>
public partial class PluginControlDisplayInfo : ViewModelBase
{
    /// <summary>
    /// 控件唯一标识符
    /// </summary>
    [ObservableProperty] private string _controlId = string.Empty;
    
    /// <summary>
    /// 控件显示名称
    /// </summary>
    [ObservableProperty] private string _displayName = string.Empty;
    
    /// <summary>
    /// 所属插件名称
    /// </summary>
    [ObservableProperty] private string _pluginName = string.Empty;
    
    /// <summary>
    /// 所属插件ID
    /// </summary>
    [ObservableProperty] private string _pluginId = string.Empty;
    
    /// <summary>
    /// 目标窗口类型
    /// </summary>
    [ObservableProperty] private FrontWindowType _targetWindowType;
    
    /// <summary>
    /// 目标窗口显示名称
    /// </summary>
    [ObservableProperty] private string _targetWindowName = string.Empty;
    
    /// <summary>
    /// 目标画布名称
    /// </summary>
    [ObservableProperty] private string _canvasName = string.Empty;
    
    /// <summary>
    /// 是否显示该控件
    /// </summary>
    [ObservableProperty] private bool _isVisible = true;
}

/// <summary>
/// 插件控件显示配置 - 保存到设置中
/// </summary>
public class PluginControlDisplayConfig
{
    /// <summary>
    /// 控件ID到显示状态的映射
    /// </summary>
    public Dictionary<string, bool> ControlVisibility { get; set; } = new();
}
