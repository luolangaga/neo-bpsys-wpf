using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using neo_bpsys_wpf.Core.Enums;
using System;
using System.Collections.Generic;
using System.Windows;

namespace neo_bpsys_wpf.Core.Abstractions.Extensions;

public interface IPlugin
{
    PluginMetadata Metadata { get; }
    void ConfigureServices(IServiceCollection services);
    void Initialize(IPluginContext context);
    IEnumerable<PluginNavigationItem> GetMenuItems();
    IEnumerable<PluginNavigationItem> GetFooterItems();
    
    /// <summary>
    /// 获取插件提供的前台自定义控件列表
    /// </summary>
    /// <returns>插件前台控件描述符列表</returns>
    IEnumerable<PluginOverlayDescriptor> GetOverlayControls() => Array.Empty<PluginOverlayDescriptor>();
    
    /// <summary>
    /// 释放插件资源，在插件被禁用或卸载时调用
    /// </summary>
    void Dispose() { }
}

public interface IPluginContext
{
    IServiceProvider Services { get; }
    IHost Host { get; }
    Application Application { get; }
    Window? MainWindow { get; }
}

public class PluginMetadata
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = "1.0.0";
    public string? Description { get; init; }
    public string? Author { get; init; }
    public bool IsEnabled { get; set; } = true;
}

public class PluginNavigationItem
{
    public string Title { get; init; } = string.Empty;
    public Type PageType { get; init; } = typeof(object);
}

/// <summary>
/// 插件前台覆盖控件描述符
/// </summary>
public class PluginOverlayDescriptor
{
    /// <summary>
    /// 控件唯一标识符，用于保存位置信息
    /// </summary>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// 控件显示名称
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;
    
    /// <summary>
    /// 控件工厂方法，返回要显示的 UIElement
    /// </summary>
    public Func<UIElement>? ControlFactory { get; init; }
    
    /// <summary>
    /// 默认 X 坐标位置
    /// </summary>
    public double DefaultLeft { get; init; } = 100;
    
    /// <summary>
    /// 默认 Y 坐标位置
    /// </summary>
    public double DefaultTop { get; init; } = 100;
    
    /// <summary>
    /// 默认宽度（可选，null 表示自动）
    /// </summary>
    public double? DefaultWidth { get; init; }
    
    /// <summary>
    /// 默认高度（可选，null 表示自动）
    /// </summary>
    public double? DefaultHeight { get; init; }

    /// <summary>
    /// 目标前台窗口类型，默认使用插件专用窗口
    /// </summary>
    public FrontWindowType TargetWindowType { get; init; } = FrontWindowType.PluginOverlayWindow;

    /// <summary>
    /// 目标画布名称，默认 BaseCanvas
    /// </summary>
    public string CanvasName { get; init; } = "BaseCanvas";
}
