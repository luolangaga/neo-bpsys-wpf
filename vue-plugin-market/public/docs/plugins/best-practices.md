# 最佳实践

总结在本项目中开发插件的常见约定与建议。

## 元数据与命名

- `Metadata.Id` 使用唯一前缀，例如 `bpsys.*`，避免冲突。
- `Version` 遵循语义化版本，配合发布管理。

## 初始化与性能

- 避免在 `Initialize` 中进行耗时阻塞操作；必要时使用异步与 `await Task.Delay(...)` 等手段在 UI 就绪后执行。
- 订阅主窗体事件前检查 `context.MainWindow != null`。

## UI 与交互

- 页面注册时使用 DI 注入 `DataContext`，保持与主程序一致的构建方式：参考 `plugins/OCR/OcrPlugin.cs:18`。
- 弹窗类型冲突时使用类型别名，避免隐式引用错误：参考 `plugins/ASG/AsgPlugin.cs:9`、`plugins/ASG/AsgPlugin.cs:10`。

## 配置与安全

- 不要在日志或配置中明文记录敏感信息（账号、密码、Token）。
- 若需要持久化，确保用户可清除并知情，避免在日志中输出。

## 热加载与禁用

- 使用扩展页命令进行热加载与禁用/启用，避免在运行时强行移除服务：`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:21`、`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:28`、`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:41`。

