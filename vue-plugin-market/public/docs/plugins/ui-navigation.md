# 菜单与页面注入

把插件页面挂到主界面左侧菜单或底部菜单，集成到导航系统。

## 注册页面与 ViewModel

在 `ConfigureServices` 注册页面与其 ViewModel，并设置 `DataContext`：

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<ViewModels.Pages.SamplePageViewModel>();
    services.AddSingleton<Views.Pages.SamplePage>(sp => new Views.Pages.SamplePage
    {
        DataContext = sp.GetRequiredService<ViewModels.Pages.SamplePageViewModel>()
    });
}
```

参考：`plugins/OCR/OcrPlugin.cs:18`。

## 返回菜单项

```csharp
public IEnumerable<PluginNavigationItem> GetMenuItems()
{
    return new[]
    {
        new PluginNavigationItem
        {
            Title = "识别助手",
            PageType = typeof(Views.Pages.SamplePage)
        }
    };
}
```

参考：`plugins/OCR/OcrPlugin.cs:33`、`plugins/OCR/OcrPlugin.cs:39`。

## 菜单重建与变更事件

应用会在插件发现与初始化后触发 `PluginsChanged`，主界面 ViewModel 可重建菜单：`neo-bpsys-wpf/ViewModels/Windows/MainWindowViewModel.cs:114`。

插件管理器在以下时机触发变更：

- 发现完成：`neo-bpsys-wpf/Extensions/PluginManager.cs:80`
- 初始化完成：`neo-bpsys-wpf/Extensions/PluginManager.cs:101`

