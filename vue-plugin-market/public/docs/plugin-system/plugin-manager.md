# 插件管理器详解

`PluginManager` 负责插件的发现、收集元数据、服务注册、初始化与通知 UI 变更。

## 关键成员

- 元数据与菜单集合：`neo-bpsys-wpf/Extensions/PluginManager.cs:16`、`neo-bpsys-wpf/Extensions/PluginManager.cs:18`、`neo-bpsys-wpf/Extensions/PluginManager.cs:21`
- 变更事件：`neo-bpsys-wpf/Extensions/PluginManager.cs:28`
- 发现方法：`neo-bpsys-wpf/Extensions/PluginManager.cs:30`
- 服务注册：`neo-bpsys-wpf/Extensions/PluginManager.cs:83`
- 初始化：`neo-bpsys-wpf/Extensions/PluginManager.cs:91`
- 热加载：`neo-bpsys-wpf/Extensions/PluginManager.cs:104`
- 禁用列表读取：`neo-bpsys-wpf/Extensions/PluginManager.cs:114`

## 发现策略

- 遍历插件目录顶层的所有 `.dll` 文件。
- 通过反射查找实现了 `IPlugin` 的类型。
- 根据 `Settings.DisabledPlugins` 过滤。

## 变更通知

- 在发现完成与初始化完成时分别触发一次 `PluginsChanged`，使 UI 刷新菜单项。

## 扩展建议

- 支持子目录扫描：可扩展 `Discover` 的遍历策略。
- 卸载钩子：如需支持“停用/卸载”，可为 `IPlugin` 增加新的生命周期方法并在热加载前调用。

