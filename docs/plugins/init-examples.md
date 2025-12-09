# 初始化场景与交互示例

在 `Initialize` 中进行运行期交互，例如订阅主窗体事件、显示弹窗等。

## 在主窗体 Loaded 弹出登录

参考 ASG 插件：

- 初始化入口：`plugins/ASG/AsgPlugin.cs:25`
- 订阅主窗体 Loaded：`plugins/ASG/AsgPlugin.cs:30`
- 弹窗登录与保存账号：`plugins/ASG/AsgPlugin.cs:59`、`plugins/ASG/AsgPlugin.cs:75`

关键点：

- 获取服务与设置：`context.Services.GetRequiredService<T>()`
- 避免命名冲突：使用 `using MessageBox = Wpf.Ui.Controls.MessageBox;`（`plugins/ASG/AsgPlugin.cs:9`）

## 访问设置并响应变更

- 读取设置：`neo-bpsys-wpf/Services/SettingsHostService.cs:70`
- 保存设置：`neo-bpsys-wpf/Services/SettingsHostService.cs:50`
- 设置模型：`neo-bpsys-wpf.Core/Models/Settings.cs:16`、`neo-bpsys-wpf.Core/Models/Settings.cs:18`、`neo-bpsys-wpf.Core/Models/Settings.cs:20`

## 异步与延时

在 UI 已加载后进行异步操作，可在 `Loaded` 事件中 `await Task.Delay(...)` 以等待界面就绪。

