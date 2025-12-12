# 📚 插件开发完整指南

> **欢迎来到插件开发世界！** 这里有你需要的所有文档。

## 🎯 新手入门路线

**如果你是第一次开发插件，请按以下顺序学习：**

### 第一阶段：基础入门 (30分钟)
1. 📖 [快速上手](getting-started.md) - 10分钟创建第一个插件
2. 🔧 [项目配置](project-setup.md) - 了解项目结构
3. 🎓 [核心概念](interfaces.md) - 理解插件工作原理

### 第二阶段：功能开发 (1-2小时)
4. 🎨 [添加菜单页面](ui-navigation.md) - 给插件添加界面
5. 🖼️ [前台组件开发](overlay-controls.md) - 在游戏界面显示自定义控件
6. 🚀 [初始化与交互](init-examples.md) - 与主程序交互

### 第三阶段：进阶技能 (1小时)
7. 🔥 [热加载与调试](hot-reload-disable.md) - 高效开发技巧
8. 🧪 [测试与调试](testing-debugging.md) - 保证插件质量
9. ✨ [最佳实践](best-practices.md) - 写出优雅的代码

### 第四阶段：问题解决
10. ❓ [常见问题 FAQ](faq.md) - 遇到问题先来这里

---

## 📋 功能索引

### 🎨 界面相关
- [添加菜单页面](ui-navigation.md) - 在主程序菜单中添加你的页面
- [前台组件开发](overlay-controls.md) - 在游戏界面显示自定义控件
- [UI集成示例](init-examples.md) - 窗口事件和界面交互

### 🔧 核心功能
- [核心接口](interfaces.md) - `IPlugin` 接口详解
- [服务注入](project-setup.md#依赖注入) - 如何使用DI容器
- [生命周期](interfaces.md#生命周期) - 插件的加载与卸载

### 🚀 开发工具
- [项目配置](project-setup.md) - `.csproj` 配置详解
- [热加载](hot-reload-disable.md) - 无需重启测试插件
- [调试技巧](testing-debugging.md) - 高效调试方法

### 💡 参考资源
- [最佳实践](best-practices.md) - 编码规范和建议
- [常见问题](faq.md) - 问题解决方案

---

## 🎓 快速参考

### 插件的基本结构

```csharp
public class MyPlugin : IPlugin
{
    // 1️⃣ 插件基本信息
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "my.plugin.id",
        Name = "我的插件",
        Version = "1.0.0"
    };
    
    // 2️⃣ 注册服务（可选）
    public void ConfigureServices(IServiceCollection services) { }
    
    // 3️⃣ 初始化
    public void Initialize(IPluginContext context) { }
    
    // 4️⃣ 返回菜单项（可选）
    public IEnumerable<PluginNavigationItem> GetMenuItems() 
    {
        return Array.Empty<PluginNavigationItem>();
    }
    
    // 5️⃣ 返回底部菜单（可选）
    public IEnumerable<PluginNavigationItem> GetFooterItems()
    {
        return Array.Empty<PluginNavigationItem>();
    }
    
    // 6️⃣ 返回前台组件（可选）
    public IEnumerable<PluginOverlayDescriptor> GetOverlayControls()
    {
        return Array.Empty<PluginOverlayDescriptor>();
    }
    
    // 7️⃣ 清理资源
    public void Dispose() { }
}
```

### 插件能做什么？

| 功能 | 说明 | 文档链接 |
|-----|------|---------|
| 🎨 添加菜单页面 | 在主程序菜单添加自定义页面 | [ui-navigation.md](ui-navigation.md) |
| 🖼️ 前台自定义控件 | 在游戏界面显示自定义UI | [overlay-controls.md](overlay-controls.md) |
| 🔧 注册服务 | 使用依赖注入管理服务 | [interfaces.md](interfaces.md) |
| 🪟 访问主窗口 | 订阅事件、弹窗等 | [init-examples.md](init-examples.md) |
| 💾 读写配置 | 保存和读取设置 | [best-practices.md](best-practices.md) |
| 🔥 热加载/卸载 | 运行时启用/禁用插件 | [hot-reload-disable.md](hot-reload-disable.md) |

---

## 📖 示例插件

系统已包含多个示例插件，可以直接参考学习：

| 插件名称 | 功能特点 | 学习重点 | 源码位置 |
|---------|---------|---------|---------|
| **OverlaySample** | 前台组件示例 | 学习如何创建前台自定义控件 | `plugins/OverlaySample/` |
| **OCR** | OCR识别助手 | 学习菜单页面和服务注册 | `plugins/OCR/` |
| **ASG** | ASG数据统计 | 学习窗口事件和数据交互 | `plugins/ASG/` |

---

## 🔍 核心概念速览

### 插件生命周期
```
发现 → 注册服务 → 初始化 → 运行 → 清理
 ↓        ↓         ↓      ↓      ↓
扫描   Configure  Initialize 正常  Dispose
DLL    Services              工作
```

### 关键接口位置
- 🔌 插件接口：`neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs`
- 🛠️ 插件管理器：`neo-bpsys-wpf/Extensions/PluginManager.cs`
- 🪟 前台服务：`neo-bpsys-wpf.Core/Abstractions/Services/IFrontService.cs`
- 🚀 应用启动：`neo-bpsys-wpf/App.xaml.cs`

---

## 💬 需要帮助？

1. 📖 先查看 [常见问题 FAQ](faq.md)
2. 🔍 查看示例插件源码
3. 📝 阅读相关章节的详细文档

---

## 🎯 下一步

选择你感兴趣的主题开始学习：

- 🆕 **从零开始？** → [快速上手](getting-started.md)
- 🎨 **想添加界面？** → [菜单页面](ui-navigation.md)
- 🖼️ **想在前台显示控件？** → [前台组件](overlay-controls.md)
- 🔧 **想深入理解？** → [核心概念](interfaces.md)
- ❓ **遇到问题？** → [常见问题](faq.md)

**祝你开发愉快！** 🎉

