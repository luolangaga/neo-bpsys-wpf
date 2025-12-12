using Microsoft.Extensions.DependencyInjection;
using neo_bpsys_wpf.Core.Abstractions.Extensions;
using neo_bpsys_wpf.Core.Enums;
using System.Windows;

namespace Bpsys.Plugin.OverlaySample;

/// <summary>
/// 示例插件：展示如何在前台窗口添加自定义控件
/// </summary>
public class OverlaySamplePlugin : IPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "bpsys.overlay-sample",
        Name = "前台控件示例插件",
        Version = "1.0.0",
        Description = "展示如何在前台窗口添加自定义控件，支持设计模式拖拽编辑",
        Author = "罗澜"
    };

    public void ConfigureServices(IServiceCollection services)
    {
        // 此示例插件不需要注册额外服务
    }

    public void Initialize(IPluginContext context)
    {
        // 初始化时可以访问主窗口等资源
    }
    
    public void Dispose()
    {
        // 清理插件资源
        // 例如：取消订阅事件、释放非托管资源等
        // 此示例插件没有需要清理的资源
    }

    public IEnumerable<PluginNavigationItem> GetMenuItems()
    {
        // 此示例插件不添加菜单项
        return Array.Empty<PluginNavigationItem>();
    }

    public IEnumerable<PluginNavigationItem> GetFooterItems()
    {
        return Array.Empty<PluginNavigationItem>();
    }

    public IEnumerable<PluginOverlayDescriptor> GetOverlayControls()
    {
        return new[]
        {
            new PluginOverlayDescriptor
            {
                Id = "OverlaySample_Timer",
                DisplayName = "倒计时器",
                ControlFactory = () => new Controls.TimerControl(),
                DefaultLeft = 100,
                DefaultTop = 100,
                DefaultWidth = 200,
                DefaultHeight = 80,
                TargetWindowType = FrontWindowType.GameDataWindow,
                CanvasName = "BaseCanvas"
            },
            new PluginOverlayDescriptor
            {
                Id = "OverlaySample_TeamScore",
                DisplayName = "队伍得分",
                ControlFactory = () => new Controls.TeamScoreControl(),
                DefaultLeft = 350,
                DefaultTop = 100,
                DefaultWidth = 300,
                DefaultHeight = 100,
                TargetWindowType = FrontWindowType.ScoreGlobalWindow,
                CanvasName = "BaseCanvas"
            },
            new PluginOverlayDescriptor
            {
                Id = "OverlaySample_Logo",
                DisplayName = "Logo 展示",
                ControlFactory = () => new Controls.LogoControl(),
                DefaultLeft = 700,
                DefaultTop = 100,
                DefaultWidth = 150,
                DefaultHeight = 150,
                TargetWindowType = FrontWindowType.WidgetsWindow,
                CanvasName = "MapV2Canvas"
            }
        };
    }
}
