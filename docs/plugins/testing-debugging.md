# 测试与调试

验证插件能被发现、菜单能显示、交互能正常工作，并定位常见错误。

## 构建命令

- 主程序：`dotnet build neo-bpsys-wpf/neo-bpsys-wpf.csproj`
- ASG 插件：`dotnet build plugins/ASG/ASG.Plugin.csproj`
- OCR 插件：`dotnet build plugins/OCR/OCR.Plugin.csproj`

## 验证要点

- 插件 DLL 是否输出到 `Plugins` 目录：`neo-bpsys-wpf/App.xaml.cs:39`
- 菜单是否显示：检查是否返回了 `PluginNavigationItem`（`neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:35`）
- 初始化是否执行：在插件 `Initialize` 中加入日志或弹窗（参考 ASG 插件）。

## 常见错误

- “Data at the root level is invalid”：文件头部有 BOM 或非法字符。重新保存为 UTF-8，或重写文件内容。
- 弹窗命名冲突：`System.Windows.MessageBox` 与 `Wpf.Ui.Controls.MessageBox` 同名，使用类型别名解决：`plugins/ASG/AsgPlugin.cs:9`、`plugins/ASG/AsgPlugin.cs:10`。
- 插件未被发现：`csproj` 输出路径不正确；检查 `OutputPath` 指向主程序 `Plugins` 目录。

## 日志定位

日志目录：`neo-bpsys-wpf.Core/AppConstants.cs:44`。应用启动时会记录关键日志，辅助定位插件发现与初始化流程。

