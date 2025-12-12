# 🔧 插件系统维护文档

> **面向系统维护者和高级开发者** - 深入了解插件系统的内部机制

## 📚 文档导航

### 🎯 推荐阅读顺序

如果你想深入理解插件系统，建议按以下顺序阅读：

1. 🏗️ [架构概览](architecture.md) - 了解系统整体设计
2. 🚀 [启动与发现流程](startup-flow.md) - 理解插件如何被发现和加载
3. 🔧 [插件管理器详解](plugin-manager.md) - 深入了解核心管理逻辑
4. ⚙️ [设置与配置持久化](settings-config.md) - 配置系统工作原理
5. 🎨 [UI 集成与菜单重建](ui-integration.md) - UI如何与插件集成
6. 🔥 [动态加载与卸载](dynamic-load-unload.md) - 热加载机制详解
7. 🐛 [故障诊断](troubleshooting.md) - 常见问题排查

### 📋 按主题查找

#### 核心机制
- [架构概览](architecture.md) - 系统组成和设计模式
- [插件管理器](plugin-manager.md) - 核心管理逻辑
- [动态加载卸载](dynamic-load-unload.md) - 热加载机制

#### 集成相关
- [启动流程](startup-flow.md) - 应用启动时的插件处理
- [UI集成](ui-integration.md) - 菜单和前台控件集成
- [设置配置](settings-config.md) - 配置持久化

#### 问题解决
- [故障诊断](troubleshooting.md) - 常见问题和解决方案

---

## 🎓 快速参考

### 关键组件位置

| 组件 | 文件位置 | 作用 |
|-----|---------|------|
| IPlugin 接口 | `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs` | 定义插件规范 |
| PluginManager | `neo-bpsys-wpf/Extensions/PluginManager.cs` | 管理插件生命周期 |
| IFrontService | `neo-bpsys-wpf.Core/Abstractions/Services/IFrontService.cs` | 前台服务接口 |
| FrontService | `neo-bpsys-wpf/Services/FrontService.cs` | 前台服务实现 |
| SettingsHostService | `neo-bpsys-wpf/Services/SettingsHostService.cs` | 设置服务 |
| App.xaml.cs | `neo-bpsys-wpf/App.xaml.cs` | 应用启动入口 |

### 关键流程

#### 插件生命周期
```
发现 → 注册服务 → 初始化 → 运行中 → 清理
 ↓        ↓          ↓        ↓       ↓
扫描    Configure  Initialize  工作   Dispose
DLL     Services
```

#### 热加载流程
```
用户启用插件
    ↓
创建实例
    ↓
Initialize
    ↓
添加UI
    ↓
更新配置
```

#### 热卸载流程
```
用户禁用插件
    ↓
调用 Dispose
    ↓
移除UI
    ↓
清理实例
    ↓
更新配置
```

---

## 🔍 核心概念

### 插件发现机制
- 扫描 `Plugins` 目录下的所有 DLL 文件
- 使用反射查找实现 `IPlugin` 接口的公共类
- 过滤掉 `settings.json` 中禁用的插件
- 缓存类型信息以提高热加载速度

### 依赖注入 (DI)
- 主程序创建全局服务容器
- 插件通过 `ConfigureServices()` 注册自己的服务
- 插件通过 `IPluginContext.Services` 获取服务
- 服务在整个应用生命周期内共享

### 前台控件管理
- 插件通过 `GetOverlayControls()` 提供控件描述符
- `FrontService` 负责将控件添加到指定窗口的指定画布
- 控件位置信息保存在 `front-positions.json`
- 支持拖拽移动和大小调整

### 配置持久化
- 主配置文件：`settings.json`
- 前台位置配置：`front-positions.json`
- 配置文件位置：`%AppData%/neo-bpsys-wpf/`
- 变更通过事件通知

---

## 🛠️ 维护任务

### 添加新的前台窗口类型

1. 在 `neo-bpsys-wpf.Core/Enums/FrontWindowType.cs` 添加枚举值
2. 在 `FrontService` 构造函数中注册窗口
3. 更新相关文档

### 添加新的插件接口方法

1. 在 `IPlugin` 接口添加方法（提供默认实现）
2. 在 `PluginManager` 中调用新方法
3. 更新示例插件
4. 更新开发文档

### 修改配置结构

1. 更新 `Settings` 模型类
2. 处理配置迁移逻辑
3. 更新 `SettingsHostService`
4. 测试向后兼容性

---

## 🐛 调试技巧

### 启用详细日志

在 `App.xaml.cs` 中添加调试输出：
```csharp
Debug.WriteLine($"发现插件: {plugin.Metadata.Name}");
```

### 查看插件加载失败原因

检查以下可能原因：
1. DLL 是否在 `Plugins` 目录
2. 类是否是 `public` 的
3. 是否正确实现了 `IPlugin` 接口
4. 依赖的程序集是否存在

### 跟踪热加载问题

在 `PluginManager.LoadPlugin()` 方法中设置断点，查看：
1. 插件实例是否成功创建
2. `Initialize()` 是否被调用
3. UI 是否被正确添加
4. 事件是否被触发

---

## 📊 性能优化

### 当前优化
- ✅ 类型信息缓存
- ✅ 延迟初始化
- ✅ 按需构建UI

### 未来可能的优化
- 🔄 异步插件加载
- 🔄 插件预热
- 🔄 更细粒度的插件隔离

---

## 🔐 安全考虑

### 当前机制
- 插件运行在相同的应用域
- 完全信任插件代码
- 不进行代码签名验证

### 建议
- ⚠️ 只加载可信来源的插件
- ⚠️ 定期审查插件代码
- ⚠️ 考虑添加插件权限系统

---

## 📝 开发者文档

如果你是插件开发者，请查看：
- 📖 [插件开发指南](../plugin-dev-guide.md)
- 🚀 [快速上手](../plugins/getting-started.md)
- 🖼️ [前台组件开发](../plugins/overlay-controls.md)

---

## ❓ 需要帮助？

1. 📖 查看 [故障诊断](troubleshooting.md)
2. 🔍 检查相关章节详细文档
3. 🐛 启用调试日志排查问题

---

**维护愉快！** 🔧

