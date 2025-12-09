# 启动与发现流程

应用启动到插件初始化的完整时序。

## 关键步骤

1. 构建 `PluginManager` 并获取插件目录：`neo-bpsys-wpf/App.xaml.cs:38`、`neo-bpsys-wpf/App.xaml.cs:39`
2. 扫描 DLL 并发现插件类型：`neo-bpsys-wpf/Extensions/PluginManager.cs:41`
3. 过滤禁用插件：`neo-bpsys-wpf/Extensions/PluginManager.cs:53`
4. 触发 `PluginsChanged`（第一次）：`neo-bpsys-wpf/Extensions/PluginManager.cs:80`
5. 构建宿主 `IHost` 并注册主程序服务：`neo-bpsys-wpf/App.xaml.cs:66`
6. 允许插件注册服务：`neo-bpsys-wpf/App.xaml.cs:206`（`_pluginManager.ConfigureServices(services)`）
7. 启动宿主并初始化插件：`neo-bpsys-wpf/App.xaml.cs:298`
8. 触发 `PluginsChanged`（第二次）：`neo-bpsys-wpf/Extensions/PluginManager.cs:101`

## 热加载流程

- 调用 `Reload()`：`neo-bpsys-wpf/Extensions/PluginManager.cs:104`
- 重新发现并再次初始化：`neo-bpsys-wpf/Extensions/PluginManager.cs:107`、`neo-bpsys-wpf/Extensions/PluginManager.cs:110`

