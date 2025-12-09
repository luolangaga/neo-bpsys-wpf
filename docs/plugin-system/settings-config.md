# 设置与配置持久化

插件系统依赖设置服务将配置持久化到 `Config.json`，并在加载时读取。

## 路径与常量

- `Config.json` 路径：`neo-bpsys-wpf.Core/AppConstants.cs:29`
- 日志目录：`neo-bpsys-wpf.Core/AppConstants.cs:44`

## 设置模型

- `ShowTip`：`neo-bpsys-wpf.Core/Models/Settings.cs:16`
- `AsgEmail`、`AsgPassword`：`neo-bpsys-wpf.Core/Models/Settings.cs:18`、`neo-bpsys-wpf.Core/Models/Settings.cs:19`
- `DisabledPlugins`：`neo-bpsys-wpf.Core/Models/Settings.cs:20`

## 设置服务

- 保存配置：`neo-bpsys-wpf/Services/SettingsHostService.cs:50`
- 加载配置：`neo-bpsys-wpf/Services/SettingsHostService.cs:70`
- 重置配置：`neo-bpsys-wpf/Services/SettingsHostService.cs:98`

## 注意事项

- 写入前确保目录存在；保存时替换系统路径以适配可移植性。
- 读取失败时回退到重置默认配置并提示错误。

