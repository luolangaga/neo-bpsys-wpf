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

