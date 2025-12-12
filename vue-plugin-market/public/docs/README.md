# 📚 插件与组件系统文档导航

> **快速找到你需要的文档！**

## 🎯 我是谁？我要看什么？

### 👨‍💻 我是插件开发新手
**目标：** 开发第一个插件

**推荐阅读路径：**
1. 🚀 [插件开发入门](plugin-dev-guide.md) - 5分钟了解插件系统
2. 📖 [快速上手](plugins/getting-started.md) - 10分钟创建第一个插件
3. 🎨 [添加菜单页面](plugins/ui-navigation.md) - 给插件添加界面
4. 🖼️ [前台组件开发](plugins/overlay-controls.md) - 在游戏界面显示自定义控件

**完整学习路径：** [插件开发索引](plugins/index.md)

---

### 🔧 我是系统维护者
**目标：** 理解和维护插件系统

**推荐阅读路径：**
1. 🏗️ [架构概览](plugin-system/architecture.md) - 理解系统设计
2. 🚀 [启动流程](plugin-system/startup-flow.md) - 了解加载机制
3. 🔧 [插件管理器](plugin-system/plugin-manager.md) - 核心管理逻辑
4. 🐛 [故障诊断](plugin-system/troubleshooting.md) - 问题排查

**完整文档：** [系统维护索引](plugin-system/index.md)

---

### 🎨 我想开发前台组件
**目标：** 在游戏界面上显示自定义UI

**直接查看：**
- 🖼️ [前台组件开发完整指南](plugins/overlay-controls.md)
- 📦 [OverlaySample 示例插件](../plugins/OverlaySample/)

**你将学会：**
- ✅ 创建WPF用户控件
- ✅ 注册前台组件
- ✅ 指定窗口和画布
- ✅ 设置默认位置和大小
- ✅ 访问服务和数据

---

### 🎯 我想添加菜单页面
**目标：** 在主程序菜单中添加自己的页面

**直接查看：**
- 📋 [菜单与页面注入](plugins/ui-navigation.md)
- 📦 [OCR 示例插件](../plugins/OCR/)

**你将学会：**
- ✅ 创建WPF页面
- ✅ 使用MVVM模式
- ✅ 注册页面和ViewModel
- ✅ 添加到菜单

---

### ❓ 我遇到了问题
**快速解决：**

1. 📖 [插件开发FAQ](plugins/faq.md) - 常见问题解答
2. 🐛 [系统故障诊断](plugin-system/troubleshooting.md) - 问题排查指南
3. 🔍 查看示例插件源码：
   - [OverlaySample](../plugins/OverlaySample/) - 前台组件示例
   - [OCR](../plugins/OCR/) - 菜单页面示例
   - [ASG](../plugins/ASG/) - 窗口交互示例

---

## 📖 完整文档目录

### 📘 插件开发文档 (面向开发者)

#### 入门教程
- [插件开发入门指南](plugin-dev-guide.md) - **从这里开始！**
- [快速上手](plugins/getting-started.md) - 10分钟创建第一个插件
- [项目配置详解](plugins/project-setup.md) - 项目结构和配置

#### 核心概念
- [核心接口与生命周期](plugins/interfaces.md) - IPlugin接口详解
- [菜单与页面注入](plugins/ui-navigation.md) - 添加UI界面
- [前台组件开发](plugins/overlay-controls.md) - 开发前台控件 ⭐**新增**
- [初始化与交互](plugins/init-examples.md) - 窗口事件和数据交互

#### 进阶技能
- [热加载与调试](plugins/hot-reload-disable.md) - 高效开发技巧
- [测试与调试](plugins/testing-debugging.md) - 保证代码质量
- [最佳实践](plugins/best-practices.md) - 编码规范

#### 参考
- [常见问题FAQ](plugins/faq.md) - 问题解答
- [插件开发索引](plugins/index.md) - 完整文档导航

---

### 📕 系统维护文档 (面向维护者)

#### 架构设计
- [架构概览](plugin-system/architecture.md) - 系统设计和组件 ⭐**已优化**
- [启动与发现流程](plugin-system/startup-flow.md) - 插件加载机制
- [插件管理器详解](plugin-system/plugin-manager.md) - 核心管理逻辑

#### 集成机制
- [UI集成与菜单](plugin-system/ui-integration.md) - UI集成详解
- [动态加载与卸载](plugin-system/dynamic-load-unload.md) - 热加载机制
- [设置与配置](plugin-system/settings-config.md) - 配置持久化

#### 维护指南
- [故障诊断](plugin-system/troubleshooting.md) - 问题排查
- [系统维护索引](plugin-system/index.md) - 完整文档导航

---

## 🎓 学习资源

### 📦 示例插件

| 插件名称 | 功能 | 学习重点 | 源码位置 |
|---------|------|---------|---------|
| **OverlaySample** | 前台控件示例 | 学习如何开发前台组件 | [plugins/OverlaySample/](../plugins/OverlaySample/) |
| **OCR** | OCR识别助手 | 学习菜单页面和服务 | [plugins/OCR/](../plugins/OCR/) |
| **ASG** | ASG数据统计 | 学习窗口事件和交互 | [plugins/ASG/](../plugins/ASG/) |

### 🔑 关键代码位置

| 组件 | 文件位置 | 说明 |
|-----|---------|------|
| IPlugin接口 | `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs` | 插件接口定义 |
| PluginManager | `neo-bpsys-wpf/Extensions/PluginManager.cs` | 插件管理器 |
| IFrontService | `neo-bpsys-wpf.Core/Abstractions/Services/IFrontService.cs` | 前台服务接口 |
| FrontService | `neo-bpsys-wpf/Services/FrontService.cs` | 前台服务实现 |
| App启动 | `neo-bpsys-wpf/App.xaml.cs` | 应用启动入口 |

---

## 🚀 快速开始

### 创建第一个插件（10分钟）

```bash
# 1. 创建插件目录
cd plugins
mkdir MyPlugin

# 2. 创建项目
dotnet new classlib -n MyPlugin

# 3. 配置项目（编辑 .csproj）
# 4. 编写插件代码（实现 IPlugin 接口）
# 5. 构建并运行
dotnet build
```

**详细步骤：** [快速上手指南](plugins/getting-started.md)

### 添加前台组件（5分钟）

```csharp
public IEnumerable<PluginOverlayDescriptor> GetOverlayControls()
{
    return new[]
    {
        new PluginOverlayDescriptor
        {
            Id = "MyPlugin_Timer",
            DisplayName = "倒计时器",
            ControlFactory = () => new MyTimerControl(),
            DefaultLeft = 100,
            DefaultTop = 100,
            TargetWindowType = FrontWindowType.GameDataWindow
        }
    };
}
```

**详细指南：** [前台组件开发](plugins/overlay-controls.md)

---

## 🎯 更新说明

### ✨ 最新更新
- 📝 **优化插件开发入门文档** - 更通俗易懂，面向新手
- 🏗️ **优化架构文档** - 添加图表和详细说明
- 🖼️ **新增前台组件开发指南** - 完整的组件开发教程
- 📚 **更新所有索引文档** - 更好的导航和分类

### 📅 文档版本
- **创建日期：** 2024年
- **最后更新：** 2024年12月12日
- **维护者：** 罗澜

---

## 💡 贡献文档

发现文档问题或有改进建议？

1. 🐛 [提交Issue](https://github.com/PLFJY/neo-bpsys-wpf/issues)
2. 📝 直接编辑文档并提交PR
3. 💬 在QQ群中反馈

---

## 📞 联系方式

- 📧 项目仓库：[GitHub](https://github.com/PLFJY/neo-bpsys-wpf)
- 💬 QQ交流群：[点击加入](https://qm.qq.com/q/uqoK5tMtJQ)
- 📖 在线文档：[查看文档](https://docs.bpsys.plfjy.top/)

---

**祝你开发愉快！** 🎉
