# FAQ 常见问题

## 插件没有出现在菜单里？

- 确认已注册页面与 ViewModel，并在 `GetMenuItems` 返回了 `PluginNavigationItem`。
- 检查插件是否被禁用：`Config.json` 的 `DisabledPlugins` 中是否包含该插件 `Id`。
- 确认 DLL 输出到主程序 `Plugins` 目录：`neo-bpsys-wpf/App.xaml.cs:39`。

## 点击“重新加载插件”后没有变化？

- 确认 `PluginManager.Reload()` 被调用：`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:21`。
- 若 UI 未刷新，检查是否订阅了 `PluginsChanged` 事件：`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:17`。

## 弹窗类型冲突怎么处理？

- 同名类型需要使用别名，例如：

```csharp
using MessageBox = Wpf.Ui.Controls.MessageBox;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;
```

参考：`plugins/ASG/AsgPlugin.cs:9`、`plugins/ASG/AsgPlugin.cs:10`。

## 构建报错 “Data at the root level is invalid”？

- 说明文件存在非法头部字符（常见是 BOM），请用 UTF-8 重新保存或重写文件内容。

## 禁用后插件服务依然存在？

- 禁用影响“发现与初始化”，不会移除已注册到 DI 的服务。如需即时停用逻辑，请在插件内部增加开关。

