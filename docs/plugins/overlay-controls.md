# 🖼️ 前台组件开发指南

> 让你的插件在游戏界面上显示自定义控件！

## 什么是前台组件？

前台组件（Overlay Controls）是指在游戏前台窗口上显示的自定义UI控件，比如：
- ⏱️ 倒计时器
- 📊 队伍得分显示
- 🏷️ Logo 展示
- 📈 实时数据统计

**特点：**
- ✅ 可以放置在不同的前台窗口上
- ✅ 支持拖拽移动和调整大小
- ✅ 位置会自动保存和恢复
- ✅ 可以在设计模式下编辑布局

---

## 🎯 快速开始

### 第一步：创建 WPF 用户控件

在你的插件项目中创建一个 WPF 用户控件：

```xml
<!-- Controls/TimerControl.xaml -->
<UserControl x:Class="MyPlugin.Controls.TimerControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Border Background="#80000000" 
            CornerRadius="8" 
            Padding="20">
        <StackPanel>
            <TextBlock Text="倒计时"
                       FontSize="16"
                       Foreground="White"
                       HorizontalAlignment="Center"/>
            <TextBlock x:Name="TimeText"
                       Text="00:00"
                       FontSize="32"
                       FontWeight="Bold"
                       Foreground="#00FF00"
                       HorizontalAlignment="Center"
                       Margin="0,10,0,0"/>
        </StackPanel>
    </Border>
</UserControl>
```

```csharp
// Controls/TimerControl.xaml.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MyPlugin.Controls;

public partial class TimerControl : UserControl
{
    private readonly DispatcherTimer _timer;
    private int _seconds = 0;

    public TimerControl()
    {
        InitializeComponent();
        
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _seconds++;
        var minutes = _seconds / 60;
        var seconds = _seconds % 60;
        TimeText.Text = $"{minutes:D2}:{seconds:D2}";
    }
}
```

### 第二步：在插件中注册组件

在你的插件类中实现 `GetOverlayControls()` 方法：

```csharp
using neo_bpsys_wpf.Core.Abstractions.Extensions;
using neo_bpsys_wpf.Core.Enums;

public class MyPlugin : IPlugin
{
    // ... 其他方法 ...
    
    public IEnumerable<PluginOverlayDescriptor> GetOverlayControls()
    {
        return new[]
        {
            new PluginOverlayDescriptor
            {
                // 🆔 唯一标识符（用于保存位置信息）
                Id = "MyPlugin_Timer",
                
                // 🏷️ 显示名称
                DisplayName = "倒计时器",
                
                // 🏭 控件工厂（返回控件实例）
                ControlFactory = () => new Controls.TimerControl(),
                
                // 📍 默认位置
                DefaultLeft = 100,
                DefaultTop = 100,
                
                // 📏 默认大小（可选，null表示自动）
                DefaultWidth = 200,
                DefaultHeight = 80,
                
                // 🪟 目标窗口（放在哪个前台窗口上）
                TargetWindowType = FrontWindowType.GameDataWindow,
                
                // 🎨 目标画布名称
                CanvasName = "BaseCanvas"
            }
        };
    }
}
```

### 第三步：构建并运行

```bash
# 构建插件
dotnet build

# 运行主程序
cd ../../neo-bpsys-wpf
dotnet run
```

✨ 你的倒计时器现在会显示在前台窗口上！

---

## 📚 详细说明

### PluginOverlayDescriptor 属性详解

| 属性 | 类型 | 必需 | 说明 |
|-----|------|------|------|
| `Id` | `string` | ✅ | 唯一标识符，用于保存/恢复位置信息 |
| `DisplayName` | `string` | ✅ | 显示名称，在设计模式下显示 |
| `ControlFactory` | `Func<UIElement>` | ✅ | 控件工厂方法，返回控件实例 |
| `DefaultLeft` | `double` | ✅ | 默认X坐标 |
| `DefaultTop` | `double` | ✅ | 默认Y坐标 |
| `DefaultWidth` | `double?` | ❌ | 默认宽度，null表示自动 |
| `DefaultHeight` | `double?` | ❌ | 默认高度，null表示自动 |
| `TargetWindowType` | `FrontWindowType` | ✅ | 目标前台窗口 |
| `CanvasName` | `string` | ✅ | 目标画布名称，默认 `"BaseCanvas"` |

### 可用的前台窗口类型

```csharp
public enum FrontWindowType
{
    BpWindow,              // BP窗口
    CutSceneWindow,        // 过场动画窗口
    GameDataWindow,        // 游戏数据窗口
    ScoreSurWindow,        // 生存模式分数窗口
    ScoreHunWindow,        // 猎人模式分数窗口
    ScoreGlobalWindow,     // 全局分数窗口
    WidgetsWindow,         // 小部件窗口
    PluginOverlayWindow    // 插件专用覆盖窗口（推荐）
}
```

💡 **建议：** 对于插件的自定义控件，推荐使用 `FrontWindowType.PluginOverlayWindow`，这是专门为插件保留的窗口。

### 可用的画布名称

不同窗口可能有不同的画布：
- `"BaseCanvas"` - 基础画布（大多数窗口都有）
- `"MapBpCanvas"` - BP地图画布（WidgetsWindow）
- `"BpOverViewCanvas"` - BP预览画布（WidgetsWindow）
- `"MapV2Canvas"` - 地图V2画布（WidgetsWindow）

---

## 🎨 高级用法

### 1. 多个组件

一个插件可以提供多个前台组件：

```csharp
public IEnumerable<PluginOverlayDescriptor> GetOverlayControls()
{
    return new[]
    {
        new PluginOverlayDescriptor
        {
            Id = "MyPlugin_Timer",
            DisplayName = "倒计时器",
            ControlFactory = () => new Controls.TimerControl(),
            DefaultLeft = 100,
            DefaultTop = 100,
            TargetWindowType = FrontWindowType.GameDataWindow
        },
        new PluginOverlayDescriptor
        {
            Id = "MyPlugin_Score",
            DisplayName = "队伍得分",
            ControlFactory = () => new Controls.ScoreControl(),
            DefaultLeft = 350,
            DefaultTop = 100,
            TargetWindowType = FrontWindowType.ScoreGlobalWindow
        },
        new PluginOverlayDescriptor
        {
            Id = "MyPlugin_Logo",
            DisplayName = "Logo",
            ControlFactory = () => new Controls.LogoControl(),
            DefaultLeft = 700,
            DefaultTop = 100,
            TargetWindowType = FrontWindowType.WidgetsWindow,
            CanvasName = "MapV2Canvas"  // 使用不同的画布
        }
    };
}
```

### 2. 数据绑定与依赖注入

如果你的控件需要访问服务，可以通过依赖注入：

```csharp
// 在 ConfigureServices 中注册服务
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IMyDataService, MyDataService>();
}

// 在 ControlFactory 中获取服务
public IEnumerable<PluginOverlayDescriptor> GetOverlayControls()
{
    return new[]
    {
        new PluginOverlayDescriptor
        {
            Id = "MyPlugin_DataDisplay",
            DisplayName = "数据显示",
            ControlFactory = () =>
            {
                // 从服务容器获取服务
                var dataService = _serviceProvider.GetService<IMyDataService>();
                return new Controls.DataDisplayControl(dataService);
            },
            DefaultLeft = 100,
            DefaultTop = 100,
            TargetWindowType = FrontWindowType.GameDataWindow
        }
    };
}

// 保存 ServiceProvider 的引用
private IServiceProvider _serviceProvider;

public void Initialize(IPluginContext context)
{
    _serviceProvider = context.Services;
}
```

### 3. 响应式设计

控件会自动支持拖拽和缩放，但你可以在设计时考虑响应式布局：

```xml
<UserControl x:Class="MyPlugin.Controls.ResponsiveControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Viewbox Stretch="Uniform">
        <!-- 使用 Viewbox 让内容自动缩放 -->
        <Border Background="#80000000" 
                CornerRadius="8" 
                Padding="20"
                Width="200"
                Height="100">
            <TextBlock Text="自适应大小" 
                       FontSize="16"
                       Foreground="White"/>
        </Border>
    </Viewbox>
</UserControl>
```

### 4. 主题样式

使用半透明背景和圆角让控件更美观：

```xml
<Border Background="#CC000000"      <!-- 80%不透明的黑色 -->
        BorderBrush="#FF00FF00"     <!-- 绿色边框 -->
        BorderThickness="2"
        CornerRadius="10"
        Padding="15">
    <!-- 你的内容 -->
</Border>
```

常用颜色代码：
- `#80000000` - 50% 不透明黑色
- `#CC000000` - 80% 不透明黑色
- `#FF00FF00` - 完全不透明绿色

---

## 💡 最佳实践

### ✅ 推荐做法

1. **使用有意义的ID**
   ```csharp
   Id = "YourPluginName_ComponentName"  // ✅ 好
   Id = "ctrl1"                          // ❌ 不好
   ```

2. **提供合理的默认大小**
   ```csharp
   DefaultWidth = 200,   // ✅ 指定合理大小
   DefaultHeight = 80
   ```

3. **使用插件专用窗口**
   ```csharp
   TargetWindowType = FrontWindowType.PluginOverlayWindow  // ✅ 推荐
   ```

4. **清理资源**
   ```csharp
   public void Dispose()
   {
       // 停止计时器、取消订阅等
       _timer?.Stop();
       _timer?.Dispose();
   }
   ```

### ❌ 避免做法

1. ❌ 不要在 `ControlFactory` 中执行耗时操作
2. ❌ 不要使用过大的默认尺寸
3. ❌ 不要忘记处理资源清理
4. ❌ 不要使用重复的 ID

---

## 🔍 调试技巧

### 查看控件是否被添加

在 `Initialize` 方法中添加日志：

```csharp
public void Initialize(IPluginContext context)
{
    var overlays = GetOverlayControls();
    foreach (var overlay in overlays)
    {
        Debug.WriteLine($"注册前台组件: {overlay.DisplayName}");
    }
}
```

### 检查控件位置

前台窗口管理器会自动保存和恢复控件位置到：
```
%AppData%/neo-bpsys-wpf/front-positions.json
```

你可以删除这个文件来重置所有位置。

---

## 📖 完整示例

查看系统自带的示例插件：

**OverlaySample 插件** - 完整的前台组件示例
- 📁 位置：`plugins/OverlaySample/`
- 📄 插件类：`OverlaySamplePlugin.cs`
- 🎨 控件示例：
  - `Controls/TimerControl.xaml` - 倒计时器
  - `Controls/TeamScoreControl.xaml` - 队伍得分
  - `Controls/LogoControl.xaml` - Logo展示

---

## ❓ 常见问题

### 控件没有显示？

**检查清单：**
1. ✅ `ControlFactory` 是否返回了有效的控件？
2. ✅ 目标窗口是否已打开？
3. ✅ ID 是否唯一？
4. ✅ 是否在正确的画布上？

### 控件位置没有保存？

确保 ID 是唯一且固定的：
```csharp
Id = "MyPlugin_MyControl"  // ✅ 固定的ID
Id = Guid.NewGuid().ToString()  // ❌ 每次都变化
```

### 如何隐藏/显示控件？

控件会在目标窗口打开时自动显示，关闭时自动隐藏。你可以通过 `IFrontService` 控制窗口：

```csharp
var frontService = context.Services.GetService<IFrontService>();
frontService.ShowWindow(FrontWindowType.GameDataWindow);
frontService.HideWindow(FrontWindowType.GameDataWindow);
```

---

**🎉 开始创建精美的前台组件吧！** 如果需要更多帮助，查看 [OverlaySample 示例插件](../../plugins/OverlaySample/)。
