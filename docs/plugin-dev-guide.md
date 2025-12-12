# 插件开发新手指南

> **欢迎！** 这份指南将手把手教你开发第一个插件，就像搭积木一样简单。

## 📚 学习路线图

**新手推荐按以下顺序学习：**

1. 📖 [快速上手](plugins/getting-started.md) - 10分钟创建你的第一个插件
2. 🔧 [项目配置](plugins/project-setup.md) - 了解项目结构和配置
3. 🎯 [核心概念](plugins/interfaces.md) - 理解插件的工作原理
4. 🎨 [添加菜单页面](plugins/ui-navigation.md) - 让你的插件有界面
5. 🖼️ [前台组件开发](plugins/overlay-controls.md) - 在游戏界面上显示自定义控件
6. 🔥 [热加载与调试](plugins/hot-reload-disable.md) - 不重启就能测试插件
7. ✅ [最佳实践](plugins/best-practices.md) - 写出优雅的插件代码
8. ❓ [常见问题](plugins/faq.md) - 遇到问题先来这里找答案

## 🎯 什么是插件？

想象一下：
- **主程序** = 手机操作系统
- **插件** = 你安装的各种APP

插件可以：
- ✨ 添加新功能（比如OCR识别、ASG数据统计）
- 🎨 添加新界面（比如自定义的设置页面）
- 🖼️ 在前台显示自定义控件（比如倒计时器、队伍得分）
- 🔌 随时启用/禁用，无需重启程序

## ⚡ 5分钟快速了解

### 你需要什么？
- ✅ Windows 10/11
- ✅ .NET 9.0 SDK
- ✅ Visual Studio 2022 或 Rider（也可以用命令行）

### 推荐依赖方式：使用 NuGet 包

从 1.0 版本起，推荐通过 NuGet 包直接引用插件开发 SDK，无需手动拷贝源码。

**安装（命令行示例）：**

```bash
dotnet add package Luolan.Bpsys.Sdk --version 1.0.0
```

如果使用本地 NuGet 源（项目仓库内的 `nuget` 文件夹），可以在 Visual Studio 的“管理 NuGet 程序包”中添加本地源并安装。推荐统一用 NuGet 包，升级和依赖管理更方便。

---

### 插件的基本结构
```csharp
// 就这么简单！实现这个接口就是一个插件了
public class MyPlugin : IPlugin
{
    // 1️⃣ 告诉系统你的插件叫什么名字
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "my.awesome.plugin",
        Name = "我的超棒插件",
        Version = "1.0.0"
    };
    
    // 2️⃣ 注册你需要的服务（可选）
    public void ConfigureServices(IServiceCollection services) { }
    
    // 3️⃣ 插件初始化时会调用这个方法
    public void Initialize(IPluginContext context) { }
    
    // 4️⃣ 返回你要添加的菜单项（可选）
    public IEnumerable<PluginNavigationItem> GetMenuItems() 
    { 
        return Array.Empty<PluginNavigationItem>(); 
    }
    
    // 5️⃣ 返回底部菜单项（可选）
    public IEnumerable<PluginNavigationItem> GetFooterItems() 
    { 
        return Array.Empty<PluginNavigationItem>(); 
    }
    
    // 6️⃣ 返回前台显示的自定义控件（可选）
    public IEnumerable<PluginOverlayDescriptor> GetOverlayControls()
    {
        return Array.Empty<PluginOverlayDescriptor>();
    }
}
```

## 🚀 马上开始

准备好了吗？点击 [快速上手](plugins/getting-started.md) 创建你的第一个插件！

---

## 📦 插件的输出位置

插件需要输出到主程序的 `Plugins` 目录，这样程序启动时才能找到并加载它。

**示例项目配置（.csproj）：**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-windows7.0</TargetFramework>
    <UseWPF>true</UseWPF>
    <!-- 输出到主程序的 Plugins 目录 -->
    <OutputPath>..\..\neo-bpsys-wpf\bin\$(Configuration)\net9.0-windows7.0\Plugins\</OutputPath>
  </PropertyGroup>
  
  <ItemGroup>
    <!-- 引用核心库 -->
    <ProjectReference Include="..\..\neo-bpsys-wpf.Core\neo-bpsys-wpf.Core.csproj" />
  </ItemGroup>
</Project>
```

💡 **提示：** 查看现有插件的 `.csproj` 文件作为参考：
- `plugins/ASG/ASG.Plugin.csproj`
- `plugins/OCR/OCR.Plugin.csproj`

---

## 🎓 深入学习

### 核心概念速览

#### 1️⃣ 插件生命周期
```
发现插件 → 注册服务 → 初始化 → 运行中 → 清理
   ↓           ↓          ↓        ↓       ↓
扫描DLL   ConfigureServices Initialize  正常工作  Dispose
```

#### 2️⃣ 插件可以做什么？

**添加菜单页面：**
```csharp
public IEnumerable<PluginNavigationItem> GetMenuItems()
{
    return new[]
    {
        new PluginNavigationItem
        {
            Title = "我的页面",
            PageType = typeof(MyPage)  // 你的WPF页面类型
        }
    };
}
```

**在前台显示自定义控件：**
```csharp
public IEnumerable<PluginOverlayDescriptor> GetOverlayControls()
{
    return new[]
    {
        new PluginOverlayDescriptor
        {
            Id = "my_timer",
            DisplayName = "倒计时器",
            ControlFactory = () => new MyTimerControl(),
            DefaultLeft = 100,
            DefaultTop = 100,
            TargetWindowType = FrontWindowType.GameDataWindow
        }
    };
}
```

**访问主窗口和服务：**
```csharp
public void Initialize(IPluginContext context)
{
    // 访问主窗口
    var mainWindow = context.MainWindow;
    
    // 获取服务
    var settings = context.Services.GetService<ISettingsHostService>();
    
    // 订阅窗口加载事件
    mainWindow.Loaded += (s, e) => 
    {
        MessageBox.Show("主窗口加载完成！");
    };
}
```

---

## 💡 实用示例

### 示例插件项目

系统已经包含了几个示例插件，可以直接参考：

| 插件名称 | 功能 | 学习重点 |
|---------|------|---------|
| **OverlaySample** | 前台控件示例 | 学习如何在前台显示自定义控件 |
| **OCR** | OCR识别助手 | 学习如何添加菜单页面和服务 |
| **ASG** | ASG数据统计 | 学习窗口事件和数据交互 |

### 查看示例代码
- 前台组件示例：`plugins/OverlaySample/OverlaySamplePlugin.cs`
- 菜单页面示例：`plugins/OCR/OcrPlugin.cs`
- 窗口交互示例：`plugins/ASG/AsgPlugin.cs`

---

## 🔍 更多资源

### 详细文档
- **插件系统架构**：`docs/plugin-system/architecture.md`
- **启动流程**：`docs/plugin-system/startup-flow.md`
- **UI集成**：`docs/plugin-system/ui-integration.md`

### 代码位置参考
- 插件接口定义：`neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs`
- 插件管理器：`neo-bpsys-wpf/Extensions/PluginManager.cs`
- 前台服务：`neo-bpsys-wpf/Services/FrontService.cs`
- 应用启动：`neo-bpsys-wpf/App.xaml.cs`

---

## ❓ 遇到问题？

1. 📖 先查看 [常见问题](plugins/faq.md)
2. 🔍 检查系统自带的示例插件
3. 📝 查看详细的接口文档

**祝你开发愉快！** 🎉
- 读取禁用列表：`neo-bpsys-wpf/Extensions/PluginManager.cs:114`
- 扩展页命令（UI 操作）：
  - 热加载：`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:21`
  - 禁用：`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:28`
  - 启用：`neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs:41`

说明：禁用/启用会影响插件的“发现与初始化”，但不会移除已注册到 DI 的服务。如果你的插件服务需要随禁用即时停用，建议在插件内部用配置开关控制逻辑，而非依赖禁用移除服务。

## 构建与运行

- 命令行示例：
  - 构建主程序：`dotnet build neo-bpsys-wpf/neo-bpsys-wpf.csproj`
  - 构建插件：`dotnet build plugins/ASG/ASG.Plugin.csproj`
- 运行主程序后，插件 DLL 应位于 `neo-bpsys-wpf/bin/<配置>/net9.0-windows7.0/Plugins/`。

## 常见问题

- 弹窗类型冲突：WPF 原生 `MessageBox` 与 WPF-UI 的 `MessageBox` 名称相同，需使用类型别名解决。参考 `plugins/ASG/AsgPlugin.cs:9`、`plugins/ASG/AsgPlugin.cs:10`。
- BOM/XAML 无效：若出现 “Data at the root level is invalid”，检查 `*.csproj` 或 `*.xaml` 是否有无效头部字符，重新保存为 UTF-8。
- 插件未被加载：检查 `csproj` 输出路径是否指向主程序 `Plugins` 目录、`Metadata.Id` 唯一且未被禁用、DLL 是否正确生成。
- 热加载不生效：确认已点击扩展页“重新加载插件”按钮；或在代码中调用 `PluginManager.Reload()`（`neo-bpsys-wpf/Extensions/PluginManager.cs:104`）。

## 开发建议

- `Metadata.Id` 建议使用前缀 `bpsys.*`，避免冲突。
- 避免在 `Initialize` 中做耗时操作；如需异步，使用 `async` 并适当延时以等待 UI 就绪。
- 不要在日志或配置中存储账号密码等敏感信息；如果必须保存，确保用户知情并可清除。
