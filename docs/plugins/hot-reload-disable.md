# 热加载与禁用

通过扩展页或代码触发插件热加载，并使用 `DisabledPlugins` 控制插件启用状态。

## 扩展页 UI 操作

- 重新加载：`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:21`
- 禁用：`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:28`
- 启用：`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:41`
- 页面按钮绑定：`neo-bpsys-wpf/Views/Pages/ExtensionPage.xaml:29`、`neo-bpsys-wpf/Views/Pages/ExtensionPage.xaml:41`、`neo-bpsys-wpf/Views/Pages/ExtensionPage.xaml:42`

## 配置与发现过滤

- 配置文件路径：`neo-bpsys-wpf.Core/AppConstants.cs:29`
- 设置字段：`neo-bpsys-wpf.Core/Models/Settings.cs:20`（`DisabledPlugins`）
- 发现过滤：`neo-bpsys-wpf/Extensions/PluginManager.cs:53`
- 热加载入口：`neo-bpsys-wpf/Extensions/PluginManager.cs:104`

## 运行时服务的注意事项

禁用/启用影响“插件的发现与初始化”，但不会移除已注册到 DI 的服务。如果需要在不重启应用的情况下停用业务逻辑，建议在插件内部使用开关判断是否执行相关逻辑。

## 代码示例：禁用/启用插件（不依赖底层实现）

```csharp
using Microsoft.Extensions.DependencyInjection;
using neo_bpsys_wpf.Core.Abstractions.Extensions;
using neo_bpsys_wpf.Core.Abstractions.Services;

public class SamplePlugin : IPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "bpsys.sample",
        Name = "示例插件"
    };

    public void Initialize(IPluginContext context)
    {
        var settingsHost = context.Services.GetRequiredService<ISettingsHostService>();
        var id = Metadata.Id;
        if (!settingsHost.Settings.DisabledPlugins.Contains(id))
        {
            settingsHost.Settings.DisabledPlugins.Add(id);
            settingsHost.SaveConfig();
        }
    }

    public void EnableSelf(IPluginContext context)
    {
        var settingsHost = context.Services.GetRequiredService<ISettingsHostService>();
        if (settingsHost.Settings.DisabledPlugins.Remove(Metadata.Id))
        {
            settingsHost.SaveConfig();
        }
    }

    public IEnumerable<PluginNavigationItem> GetMenuItems() => Array.Empty<PluginNavigationItem>();
    public IEnumerable<PluginNavigationItem> GetFooterItems() => Array.Empty<PluginNavigationItem>();
    public void ConfigureServices(IServiceCollection services) {}
}
```

## 代码示例：提示用户点击扩展页进行热重载

```csharp
using Microsoft.Extensions.DependencyInjection;
using neo_bpsys_wpf.Core.Abstractions.Extensions;
using neo_bpsys_wpf.Core.Abstractions.Services;

public class SamplePlugin : IPlugin
{
    public PluginMetadata Metadata { get; } = new() { Id = "bpsys.sample" };

    public async void Initialize(IPluginContext context)
    {
        var messageBox = context.Services.GetRequiredService<IMessageBoxService>();
        await messageBox.ShowInfoAsync("插件设置已更新，请在扩展页点击‘重新加载插件’");
    }

    public IEnumerable<PluginNavigationItem> GetMenuItems() => Array.Empty<PluginNavigationItem>();
    public IEnumerable<PluginNavigationItem> GetFooterItems() => Array.Empty<PluginNavigationItem>();
    public void ConfigureServices(IServiceCollection services) {}
}
```

## 代码示例：直接热重载（需要引用主程序包）

```csharp
using Microsoft.Extensions.DependencyInjection;
using neo_bpsys_wpf.Core.Abstractions.Extensions;
using neo_bpsys_wpf.Extensions; // 需要插件引用 neo-bpsys-wpf 项目

public class SamplePlugin : IPlugin
{
    public PluginMetadata Metadata { get; } = new() { Id = "bpsys.sample" };

    public void Initialize(IPluginContext context)
    {
        var pm = context.Services.GetRequiredService<PluginManager>();
        pm.Reload();
    }

    public IEnumerable<PluginNavigationItem> GetMenuItems() => Array.Empty<PluginNavigationItem>();
    public IEnumerable<PluginNavigationItem> GetFooterItems() => Array.Empty<PluginNavigationItem>();
    public void ConfigureServices(IServiceCollection services) {}
}
```
