# UI 集成与菜单重建

插件通过返回 `PluginNavigationItem` 注入菜单项，主界面在插件变更事件触发时重建导航。

## 插件返回菜单项

- 返回主菜单项：`neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:14`
- 返回底部菜单项：`neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:15`
- 示例：`plugins/OCR/OcrPlugin.cs:33`、`plugins/OCR/OcrPlugin.cs:39`

## 主界面重建菜单

- 初始化后构建一次菜单。
- 订阅 `PluginsChanged` 后在插件变更时重建：`neo-bpsys-wpf/ViewModels/Windows/MainWindowViewModel.cs:114`

## 插件管理页面

- 扩展页提供热加载与禁用/启用按钮：
  - 页面：`neo-bpsys-wpf/Views/Pages/ExtensionPage.xaml:29`、`neo-bpsys-wpf/Views/Pages/ExtensionPage.xaml:41`、`neo-bpsys-wpf/Views/Pages/ExtensionPage.xaml:42`
  - 命令：`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:21`、`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:28`、`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:41`

