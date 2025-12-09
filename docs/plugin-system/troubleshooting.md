# 故障诊断

维护插件系统时常见的问题与处理方法。

## 构建报错：XML 无效

- 错误信息：“Data at the root level is invalid. Line 1, position 1.”
- 原因：文件头有 BOM 或非法字符。
- 处理：用 UTF-8 重新保存或重写文件内容（`*.csproj`、`*.xaml` 常见）。

## 弹窗类型冲突

- 同名类型 `MessageBox`/`MessageBoxResult` 来自多个命名空间。
- 处理：使用类型别名，参考 `plugins/ASG/AsgPlugin.cs:9`、`plugins/ASG/AsgPlugin.cs:10`。

## 菜单不刷新

- 检查是否订阅了 `PluginsChanged`：`neo-bpsys-wpf/ViewModels/Windows/MainWindowViewModel.cs:114`。
- 手动触发热加载：`neo-bpsys-wpf/Extensions/PluginManager.cs:104`。

## 插件未加载

- 确认 DLL 输出到 `Plugins` 目录：`neo-bpsys-wpf/App.xaml.cs:39`。
- 检查是否被禁用：`neo-bpsys-wpf.Core/Models/Settings.cs:20`。

## 设置读写异常

- 保存：`neo-bpsys-wpf/Services/SettingsHostService.cs:50`
- 读取：`neo-bpsys-wpf/Services/SettingsHostService.cs:70`

