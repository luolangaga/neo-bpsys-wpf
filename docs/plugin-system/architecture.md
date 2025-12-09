# 架构概览

插件系统由以下核心组件构成：

- 插件接口：`IPlugin` 与 `IPluginContext`（`neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:9`）
- 插件管理器：`PluginManager`（`neo-bpsys-wpf/Extensions/PluginManager.cs:14`）
- 启动流程与宿主：`App.xaml.cs`（`neo-bpsys-wpf/App.xaml.cs:36`）
- 设置服务与配置：`SettingsHostService` 与 `Settings`（`neo-bpsys-wpf/Services/SettingsHostService.cs:16`、`neo-bpsys-wpf.Core/Models/Settings.cs:14`）

职责分工：

- 插件接口：声明能力与生命周期钩子。
- 管理器：扫描、加载、注册、初始化、通知 UI 变更。
- 宿主：创建 DI 容器，协调插件服务注册与初始化时序。
- 设置：提供配置文件读写与变更通知。

