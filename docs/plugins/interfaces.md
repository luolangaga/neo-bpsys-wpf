# 核心接口与生命周期

了解 `IPlugin` 与 `IPluginContext`，掌握插件初始化、服务注册与菜单注入的生命周期。

## `IPlugin`

位置：`neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:9`

关键成员：

- `PluginMetadata Metadata`：插件元信息（`Id`、`Name`、`Version`、`Author` 等）`neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:26`
- `void ConfigureServices(IServiceCollection services)`：注册服务到 DI 容器 `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:12`
- `void Initialize(IPluginContext context)`：运行时初始化（可访问 `MainWindow`、`Application`、`Services`）`neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:13`
- `IEnumerable<PluginNavigationItem> GetMenuItems()`：返回菜单项 `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:14`
- `IEnumerable<PluginNavigationItem> GetFooterItems()`：返回底部菜单项 `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:15`

## `IPluginContext`

位置：`neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:18`

- `IServiceProvider Services`：获取已构建的 DI 容器 `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:20`
- `IHost Host`：获取宿主应用 `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:21`
- `Application Application`：WPF 应用对象 `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:22`
- `Window? MainWindow`：主窗体（可能为空）`neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:23`

## 生命周期时序

1. 应用启动，扫描 `Plugins` 目录发现插件：`neo-bpsys-wpf/App.xaml.cs:39`
2. 宿主构建阶段，调用 `PluginManager.ConfigureServices` 让插件注册服务：`neo-bpsys-wpf/App.xaml.cs:206`
3. 宿主启动完成，调用 `PluginManager.Initialize` 让插件执行运行时初始化：`neo-bpsys-wpf/App.xaml.cs:298`、`neo-bpsys-wpf/Extensions/PluginManager.cs:91`
4. 初始化后触发 `PluginsChanged`，UI 可重建菜单：`neo-bpsys-wpf/Extensions/PluginManager.cs:101`

## 示例参考

- OCR 插件注册页面与菜单：`plugins/OCR/OcrPlugin.cs:18`、`plugins/OCR/OcrPlugin.cs:33`
- ASG 插件在主窗体 Loaded 弹出登录：`plugins/ASG/AsgPlugin.cs:30`、`plugins/ASG/AsgPlugin.cs:59`

