# 插件控件管理功能

## 概述

插件控件管理功能允许您查看和管理所有插件提供的前台控件,控制它们在各个窗口中的显示与隐藏。

## 功能特性

### 1. 控件列表展示
- 显示所有已加载插件提供的前台控件
- 查看每个控件的详细信息:
  - 控件名称
  - 所属插件
  - 目标窗口
  - 画布位置
  - 唯一ID

### 2. 统计信息
- **总控件数**: 显示当前加载的所有插件控件数量
- **已显示**: 当前显示在前台的控件数量
- **已隐藏**: 被隐藏的控件数量

### 3. 控件管理操作
- **显示/隐藏切换**: 每个控件都有一个开关,可以实时控制其显示状态
- **刷新列表**: 重新加载插件控件列表
- **全部显示**: 一键显示所有控件
- **全部隐藏**: 一键隐藏所有控件
- **重置为默认**: 恢复所有控件到默认显示状态

### 4. 搜索功能
- 支持按控件名称或插件名称搜索过滤(待实现)

## 使用方法

### 访问控件管理页面
1. 打开主窗口
2. 在底部导航栏点击 "插件控件" 菜单项
3. 进入插件控件管理页面

### 控制控件显示
1. 在控件列表中找到需要管理的控件
2. 点击右侧的开关按钮
3. 控件会立即在对应的前台窗口显示或隐藏

### 批量操作
- 点击 "全部显示" 按钮显示所有控件
- 点击 "全部隐藏" 按钮隐藏所有控件
- 点击 "重置为默认" 按钮恢复默认状态

## 配置持久化

控件的显示状态会自动保存到应用程序设置中,下次启动应用时会自动恢复上次的配置。

配置保存在 `Settings.json` 文件的 `PluginControlDisplayConfig` 部分:

```json
{
  "PluginControlDisplayConfig": {
    "ControlVisibility": {
      "PluginId.ControlName": true,
      "AnotherPlugin.ControlName": false
    }
  }
}
```

## 技术实现

### 核心文件

1. **数据模型**
   - `neo-bpsys-wpf.Core/Models/PluginControlDisplayInfo.cs` - 控件显示信息模型
   - `neo-bpsys-wpf.Core/Models/Settings.cs` - 添加了 `PluginControlDisplayConfig` 配置

2. **ViewModel**
   - `neo-bpsys-wpf/ViewModels/Pages/PluginControlManagePageViewModel.cs` - 页面逻辑

3. **View**
   - `neo-bpsys-wpf/Views/Pages/PluginControlManagePage.xaml` - UI界面
   - `neo-bpsys-wpf/Views/Pages/PluginControlManagePage.xaml.cs` - 代码后端

4. **转换器**
   - `neo-bpsys-wpf/Converters/BoolToTextConverter.cs` - 布尔值到文本转换

5. **服务集成**
   - `neo-bpsys-wpf/Services/FrontService.cs` - 添加了配置检查逻辑
   - `neo-bpsys-wpf/App.xaml.cs` - 注册了页面和ViewModel

## 窗口类型

控件可以显示在以下窗口中:
- BP窗口
- 过场动画窗口
- 比分窗口(求生者/监管者/全局)
- 游戏数据窗口
- 小部件窗口
- 插件覆盖窗口

## 注意事项

1. 控件的显示状态更改会立即生效
2. 配置会自动保存,无需手动保存
3. 插件重新加载后需要点击"刷新列表"按钮更新显示
4. 隐藏的控件不会从配置中删除,可以随时恢复显示

## 开发者指南

### 为插件添加可管理的控件

在插件中实现 `GetOverlayControls()` 方法:

```csharp
public IEnumerable<PluginOverlayDescriptor> GetOverlayControls()
{
    return new[]
    {
        new PluginOverlayDescriptor
        {
            Id = "YourPlugin.ControlName",  // 唯一ID
            DisplayName = "控件显示名称",
            ControlFactory = () => new YourControl(),
            DefaultLeft = 100,
            DefaultTop = 100,
            TargetWindowType = FrontWindowType.GameDataWindow,
            CanvasName = "BaseCanvas"
        }
    };
}
```

### 控件ID命名规范

建议使用 `PluginId.ControlName` 格式,例如:
- `bpsys.overlay-sample.Timer`
- `YourPlugin.StatusDisplay`

这样可以方便地识别控件所属的插件。
