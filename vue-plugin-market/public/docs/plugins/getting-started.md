# 快速上手

本章用最少步骤创建一个可加载的插件，并把页面挂到主界面菜单。

## 1. 创建项目

- 在仓库 `plugins/YourPlugin` 下新建 `Class Library` 项目。
- 目标框架设为 `net9.0-windows7.0`，启用 `UseWPF`。
- 引用 Core 工程：`neo-bpsys-wpf.Core/neo-bpsys-wpf.Core.csproj`。

示例 `csproj`：详见 [项目与输出配置](project-setup.md)。

## 2. 实现插件接口

创建 `YourPlugin.cs`：

```csharp
using Microsoft.Extensions.DependencyInjection;
using neo_bpsys_wpf.Core.Abstractions.Extensions;

namespace Bpsys.Plugin.YourPlugin;

public class YourPlugin : IPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "bpsys.your",
        Name = "你的第一个插件",
        Version = "1.0.0",
        Description = "演示插件开发",
        Author = "你"
    };

    public void ConfigureServices(IServiceCollection services)
    {
        // 注册你的服务到 DI 容器
    }

    public void Initialize(IPluginContext context)
    {
        // 可订阅主窗体 Loaded 等事件或读取设置
    }

    public IEnumerable<PluginNavigationItem> GetMenuItems()
    {
        return Array.Empty<PluginNavigationItem>();
    }

    public IEnumerable<PluginNavigationItem> GetFooterItems()
    {
        return Array.Empty<PluginNavigationItem>();
    }
}
```

接口定义参考：`neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:11`。

## 3. 注册一个页面并挂菜单

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<ViewModels.Pages.SamplePageViewModel>();
    services.AddSingleton<Views.Pages.SamplePage>(sp => new Views.Pages.SamplePage
    {
        DataContext = sp.GetRequiredService<ViewModels.Pages.SamplePageViewModel>()
    });
}

public IEnumerable<PluginNavigationItem> GetMenuItems()
{
    return new[]
    {
        new PluginNavigationItem
        {
            Title = "示例页面",
            PageType = typeof(Views.Pages.SamplePage)
        }
    };
}
```

完整参考：`plugins/OCR/OcrPlugin.cs:18`、`plugins/OCR/OcrPlugin.cs:33`、`plugins/OCR/OcrPlugin.cs:39`。

## 4. 构建并运行

- 构建插件：`dotnet build plugins/YourPlugin/YourPlugin.csproj`
- 启动主程序：`dotnet build neo-bpsys-wpf/neo-bpsys-wpf.csproj`
- 插件 DLL 应输出到：`neo-bpsys-wpf/bin/<配置>/net9.0-windows7.0/Plugins/`

主程序启动时会自动发现并初始化插件：`neo-bpsys-wpf/App.xaml.cs:39`、`neo-bpsys-wpf/App.xaml.cs:298`。

## 5. 常见问题

- 菜单未出现：检查是否注册了页面与 ViewModel，并返回了 `PluginNavigationItem`。
- 插件未被发现：检查 `csproj` 输出路径是否指向主程序 `Plugins` 目录。
- 命名冲突：WPF 原生 `MessageBox` 与 WPF-UI 名称相同时，请使用类型别名。

