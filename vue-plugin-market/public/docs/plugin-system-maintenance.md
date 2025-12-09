# 插件系统维护新手指南
新手维护者建议先阅读分章索引：
- 维护索引：`docs/plugin-system/index.md`
- 架构概览：`docs/plugin-system/architecture.md`
- 启动与发现流程：`docs/plugin-system/startup-flow.md`
- 插件管理器详解：`docs/plugin-system/plugin-manager.md`
- 设置与配置持久化：`docs/plugin-system/settings-config.md`
- UI 集成与菜单重建：`docs/plugin-system/ui-integration.md`
- 故障诊断：`docs/plugin-system/troubleshooting.md`

本指南面向需要维护本项目“插件系统”的开发者，讲解系统架构、启动流程、热加载/禁用机制、UI 集成以及常见问题定位方法。

## 架构概览

- 管理器：`PluginManager` 负责插件扫描、元数据收集、服务注册、初始化与变更事件通知。
- 插件接口：`IPlugin` 定义插件的生命周期与能力（服务注册、初始化、菜单/页脚注入）。
- 依赖注入：主程序构建宿主（`IHost`），在构建阶段末尾调用 `PluginManager.ConfigureServices` 让插件注册服务。
- UI 集成：主窗体与 ViewModel 从 `PluginManager` 读取菜单项并响应变更事件重建菜单。
- 配置持久化：通过 `SettingsHostService` 读写 `Config.json`，其中包含 `DisabledPlugins` 等字段。

## 启动与发现流程

应用启动时的关键步骤与代码位置：

- 创建并发现插件目录：`neo-bpsys-wpf/App.xaml.cs:39`、`neo-bpsys-wpf/App.xaml.cs:40`
- 宿主构建阶段，让插件注册服务：`neo-bpsys-wpf/App.xaml.cs:206`
- 宿主启动后，初始化插件并广播变更：`neo-bpsys-wpf/App.xaml.cs:298`

`PluginManager` 发现与初始化的关键实现：

- 变更事件声明：`neo-bpsys-wpf/Extensions/PluginManager.cs:28`
- 发现并过滤禁用插件：`neo-bpsys-wpf/Extensions/PluginManager.cs:30`、`neo-bpsys-wpf/Extensions/PluginManager.cs:53`
- 发现后通知 UI：`neo-bpsys-wpf/Extensions/PluginManager.cs:80`
- 初始化插件与通知 UI：`neo-bpsys-wpf/Extensions/PluginManager.cs:91`、`neo-bpsys-wpf/Extensions/PluginManager.cs:101`

## 热加载与禁用机制

- 配置文件位置：`neo-bpsys-wpf.Core/AppConstants.cs:29`
- 禁用列表字段：`neo-bpsys-wpf.Core/Models/Settings.cs:20`
- 读取禁用列表：`neo-bpsys-wpf/Extensions/PluginManager.cs:114`
- 热加载入口：`neo-bpsys-wpf/Extensions/PluginManager.cs:104`

UI 层的管理命令：

- 热加载命令：`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:21`
- 禁用命令：`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:28`
- 启用命令：`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:41`

注意：禁用/启用只影响“发现与初始化”，不会在运行时移除已注册到 DI 容器的服务。若需即时关闭插件功能，建议插件内部通过配置开关控制逻辑。

## UI 集成与菜单重建

- 插件向菜单注入条目：`IPlugin.GetMenuItems` 返回 `PluginNavigationItem`（`neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:14`）。
- 主界面 ViewModel 构建菜单并监听插件变化：`neo-bpsys-wpf/ViewModels/Windows/MainWindowViewModel.cs:114`。
- OCR 插件页面注入参考：`plugins/OCR/OcrPlugin.cs:18`、`plugins/OCR/OcrPlugin.cs:33`、`plugins/OCR/OcrPlugin.cs:39`。

## 依赖注入时序与规则

- 主程序服务优先注册，最后调用 `PluginManager.ConfigureServices` 允许插件覆盖或补充：`neo-bpsys-wpf/App.xaml.cs:206`。
- 插件初始化在宿主启动后执行，以便使用 `IServiceProvider`、`MainWindow` 等运行时对象：`neo-bpsys-wpf/Extensions/PluginManager.cs:91`。

## 设置与持久化

- 设置模型：`neo-bpsys-wpf.Core/Models/Settings.cs:16`（`ShowTip`）、`neo-bpsys-wpf.Core/Models/Settings.cs:18`（`AsgEmail`）、`neo-bpsys-wpf.Core/Models/Settings.cs:19`（`AsgPassword`）、`neo-bpsys-wpf.Core/Models/Settings.cs:20`（`DisabledPlugins`）。
- 保存设置：`neo-bpsys-wpf/Services/SettingsHostService.cs:50`（写入 `Config.json`）。
- 读取设置：`neo-bpsys-wpf/Services/SettingsHostService.cs:70`。

## 维护建议

- 统一 ID 命名：建议以 `bpsys.*` 前缀，避免冲突。
- 变更通知：任何影响菜单的操作（发现、初始化、热加载）都应触发 `PluginsChanged`。
- 异常隔离：在扫描/初始化时捕获并隔离异常，避免单个插件影响全局。（参考 `PluginManager` 中的 `try { ... } catch { }`）。
- 性能考虑：避免在插件初始化中执行耗时阻塞操作；必要时使用异步与延时。
- 安全实践：不要记录或暴露敏感信息（如账号密码、Token）。

## 故障诊断

常见问题与定位方法：

- 构建报错 “Data at the root level is invalid”：检查 `*.csproj` 或 `*.xaml` 是否含有 BOM 或非法字符，重新保存为 UTF-8。
- 插件未显示：确认插件 DLL 是否落到主程序 `Plugins` 目录、`Metadata.Id` 唯一且未在 `DisabledPlugins` 中、插件是否抛异常被忽略。
- 菜单不更新：检查是否订阅了 `PluginsChanged` 事件；可调用 `PluginManager.Reload()` 强制热加载（`neo-bpsys-wpf/Extensions/PluginManager.cs:104`）。
- 弹窗类型冲突：WPF 原生 `MessageBox` 与 WPF-UI 同名，需使用别名，参考 `plugins/ASG/AsgPlugin.cs:9`、`plugins/ASG/AsgPlugin.cs:10`。
- 配置未保存：确认调用了 `SettingsHostService.SaveConfig`（`neo-bpsys-wpf/Services/SettingsHostService.cs:50`）。

日志位置：`neo-bpsys-wpf.Core/AppConstants.cs:44` 指定日志目录；应用启动时初始化日志文件。

## 发布与版本管理

- 输出路径：统一将插件输出到主程序的 `Plugins` 目录，确保能被发现。
- 版本号：遵循语义化版本（`Major.Minor.Patch`），在 `PluginMetadata.Version` 中维护。
- 回归验证：最小验证包括加载、菜单注入、热加载与禁用操作的完整流程。

## 扩展点与演进

- 元数据扩展：如需新增字段，可在 `PluginMetadata` 中添加，并同步维护 UI 展示逻辑。
- 发现策略：当前仅扫描顶层 DLL，如需支持子目录，可扩展 `Discover` 的遍历策略。
- 生命周期钩子：如需更多事件（卸载、暂停），可在 `IPlugin` 中新增方法并调整 `PluginManager` 调用时序。
