# 🚀 快速上手

> **目标：** 10分钟创建你的第一个插件！

## 第一步：创建项目 📁

### 1. 在仓库中创建插件目录
```bash
# 在 plugins 目录下创建你的插件目录
cd plugins
mkdir MyFirstPlugin
cd MyFirstPlugin
```

### 2. 创建 Class Library 项目
```bash
dotnet new classlib -n MyFirstPlugin
```

### 3. 配置项目文件

编辑 `MyFirstPlugin.csproj`，替换为以下内容：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- 🎯 目标框架 -->
    <TargetFramework>net9.0-windows7.0</TargetFramework>
    
    <!-- ✨ 启用 WPF（如果需要UI功能） -->
    <UseWPF>true</UseWPF>
    
    <!-- 📦 输出到主程序的 Plugins 目录 -->
    <OutputPath>..\..\neo-bpsys-wpf\bin\$(Configuration)\net9.0-windows7.0\Plugins\</OutputPath>
    
    <!-- 🔧 其他设置 -->
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <!-- 🔌 引用核心库（必需） -->
    <ProjectReference Include="..\..\neo-bpsys-wpf.Core\neo-bpsys-wpf.Core.csproj" />
  </ItemGroup>
</Project>
```

💡 **关键配置说明：**
- `TargetFramework`：必须是 `net9.0-windows7.0`
- `UseWPF`：如果你要创建UI界面，必须设为 `true`
- `OutputPath`：插件DLL必须输出到主程序的 `Plugins` 目录

---

## 第二步：编写插件代码 💻

### 创建插件类

删除默认的 `Class1.cs`，创建 `MyFirstPlugin.cs`：

```csharp
using Microsoft.Extensions.DependencyInjection;
using neo_bpsys_wpf.Core.Abstractions.Extensions;
using System.Windows;

namespace MyFirstPlugin;

/// <summary>
/// 我的第一个插件
/// </summary>
public class MyFirstPlugin : IPlugin
{
    // 🏷️ 插件的基本信息
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "my.first.plugin",              // 唯一ID（建议用域名格式）
        Name = "我的第一个插件",             // 显示名称
        Version = "1.0.0",                   // 版本号
        Description = "这是我开发的第一个插件！", // 描述
        Author = "你的名字"                   // 作者
    };

    // 🔧 注册服务（暂时不需要）
    public void ConfigureServices(IServiceCollection services)
    {
        // 这里可以注册你的服务到依赖注入容器
        // 例如：services.AddSingleton<IMyService, MyService>();
    }

    // 🚀 初始化插件
    public void Initialize(IPluginContext context)
    {
        // 插件被加载时会调用这个方法
        // 让我们弹个窗口证明插件成功加载了！
        
        // 等主窗口加载完成后再弹窗
        if (context.MainWindow != null)
        {
            context.MainWindow.Loaded += (s, e) =>
            {
                MessageBox.Show(
                    "🎉 恭喜！你的第一个插件成功加载了！",
                    "插件加载成功",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            };
        }
    }

    // 📋 添加菜单项（暂时不需要）
    public IEnumerable<PluginNavigationItem> GetMenuItems()
    {
        // 返回空列表，表示不添加菜单
        return Array.Empty<PluginNavigationItem>();
    }

    // 📌 添加底部菜单项（暂时不需要）
    public IEnumerable<PluginNavigationItem> GetFooterItems()
    {
        return Array.Empty<PluginNavigationItem>();
    }
    
    // 🖼️ 添加前台控件（暂时不需要）
    public IEnumerable<PluginOverlayDescriptor> GetOverlayControls()
    {
        return Array.Empty<PluginOverlayDescriptor>();
    }

    // 🧹 清理资源
    public void Dispose()
    {
        // 插件被卸载时会调用这个方法
        // 在这里释放资源、取消事件订阅等
    }
}
```

---

## 第三步：构建并测试 🏗️

### 1. 构建插件
```bash
# 在插件目录下
dotnet build
```

✅ 如果构建成功，你会在 `neo-bpsys-wpf\bin\Debug\net9.0-windows7.0\Plugins\` 目录下看到 `MyFirstPlugin.dll`

### 2. 启动主程序
```bash
# 返回到主项目目录
cd ..\..\neo-bpsys-wpf

# 运行主程序
dotnet run
```

### 3. 看到效果 🎉

主程序启动后，如果一切正常，你会看到一个消息框：
```
🎉 恭喜！你的第一个插件成功加载了！
```

---

## 🎓 理解代码

### IPlugin 接口的关键方法

| 方法 | 作用 | 何时调用 |
|-----|------|---------|
| `Metadata` | 插件的基本信息 | 扫描插件时读取 |
| `ConfigureServices` | 注册服务到DI容器 | 应用启动时 |
| `Initialize` | 初始化插件 | 应用启动后 |
| `GetMenuItems` | 返回菜单项 | 构建菜单时 |
| `GetFooterItems` | 返回底部菜单项 | 构建菜单时 |
| `GetOverlayControls` | 返回前台控件 | 初始化前台窗口时 |
| `Dispose` | 清理资源 | 插件被禁用/卸载时 |

### IPluginContext - 插件的上下文

在 `Initialize` 方法中，你可以通过 `context` 访问：

```csharp
public void Initialize(IPluginContext context)
{
    // 🪟 获取主窗口
    var mainWindow = context.MainWindow;
    
    // 🏠 获取应用程序对象
    var app = context.Application;
    
    // 🔧 获取服务容器（可以获取任何已注册的服务）
    var settingsService = context.Services.GetService<ISettingsHostService>();
    
    // 🖥️ 获取主机对象
    var host = context.Host;
}
```

---

## 🎯 下一步

恭喜完成第一个插件！接下来你可以：

1. 📖 [添加菜单页面](ui-navigation.md) - 给插件添加自己的界面
2. 🖼️ [开发前台组件](overlay-controls.md) - 在游戏界面显示自定义控件
3. 🔧 [项目配置详解](project-setup.md) - 深入了解项目配置
4. 🎯 [核心概念](interfaces.md) - 理解插件系统的工作原理

---

## ❓ 常见问题

### 插件没有被加载？

**检查清单：**
1. ✅ DLL是否输出到了 `Plugins` 目录？
2. ✅ 类是否实现了 `IPlugin` 接口？
3. ✅ 类是否是 `public` 的？
4. ✅ 项目是否引用了 `neo-bpsys-wpf.Core`？

### 弹窗没有出现？

可能是主窗口还没加载完成。确保在 `MainWindow.Loaded` 事件中弹窗：
```csharp
context.MainWindow.Loaded += (s, e) => 
{
    MessageBox.Show("现在弹窗！");
};
```

### 构建失败？

检查：
1. ✅ .NET 9.0 SDK 是否已安装？
2. ✅ 项目引用路径是否正确？
3. ✅ 查看错误信息，通常会告诉你哪里出了问题

---

**🎉 做得好！** 你已经创建了第一个插件！继续探索更多功能吧！

