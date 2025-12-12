# 🏗️ 插件系统架构概览

> 面向维护者和想深入理解系统的开发者

## 系统组成

插件系统就像一个"应用商店"，由以下核心部分组成：

```
┌─────────────────────────────────────────────┐
│           主程序 (neo-bpsys-wpf)            │
│  ┌───────────────────────────────────────┐  │
│  │      插件管理器 (PluginManager)       │  │
│  │   - 发现插件                          │  │
│  │   - 加载/卸载插件                     │  │
│  │   - 管理插件生命周期                  │  │
│  └───────────────────────────────────────┘  │
│  ┌───────────────────────────────────────┐  │
│  │    前台服务 (FrontService)            │  │
│  │   - 管理前台窗口                      │  │
│  │   - 添加/移除插件控件                 │  │
│  └───────────────────────────────────────┘  │
│  ┌───────────────────────────────────────┐  │
│  │    设置服务 (SettingsHostService)     │  │
│  │   - 保存/加载配置                     │  │
│  │   - 管理禁用插件列表                  │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
                    ↓ 使用
┌─────────────────────────────────────────────┐
│         核心库 (neo-bpsys-wpf.Core)         │
│  ┌───────────────────────────────────────┐  │
│  │    插件接口 (IPlugin)                 │  │
│  │   - 定义插件规范                      │  │
│  │   - 生命周期钩子                      │  │
│  └───────────────────────────────────────┘  │
│  ┌───────────────────────────────────────┐  │
│  │    插件上下文 (IPluginContext)        │  │
│  │   - 提供主窗口访问                    │  │
│  │   - 提供服务访问                      │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
                    ↑ 实现
┌─────────────────────────────────────────────┐
│              各种插件 (Plugins)             │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐     │
│  │   OCR   │  │   ASG   │  │  自定义 │     │
│  │  插件   │  │  插件   │  │  插件   │ ... │
│  └─────────┘  └─────────┘  └─────────┘     │
└─────────────────────────────────────────────┘
```

## 核心组件详解

### 1. 插件接口 (IPlugin)
**位置：** `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs`

**作用：** 定义插件必须实现的规范

**关键方法：**
- `PluginMetadata Metadata` - 插件元数据（ID、名称、版本等）
- `ConfigureServices()` - 注册服务到DI容器
- `Initialize()` - 初始化插件
- `GetMenuItems()` - 提供菜单项
- `GetFooterItems()` - 提供底部菜单项
- `GetOverlayControls()` - 提供前台控件
- `Dispose()` - 清理资源

**类比：** 就像USB接口标准，所有U盘都必须遵循这个标准才能被电脑识别。

### 2. 插件管理器 (PluginManager)
**位置：** `neo-bpsys-wpf/Extensions/PluginManager.cs`

**作用：** 管理所有插件的生命周期

**主要职责：**
1. 🔍 **发现插件** - 扫描 `Plugins` 目录下的所有 DLL
2. ⚙️ **服务注册** - 调用插件的 `ConfigureServices` 注册服务
3. 🚀 **初始化** - 调用插件的 `Initialize` 方法
4. 🔥 **热加载** - 运行时加载新插件
5. ❌ **热卸载** - 运行时卸载插件
6. 📢 **事件通知** - 通知UI插件状态变化

**关键方法：**
```csharp
- DiscoverPlugins()      // 发现插件
- ConfigureServices()    // 让插件注册服务
- Initialize()           // 初始化所有插件
- LoadPlugin()           // 热加载单个插件
- UnloadPlugin()         // 热卸载单个插件
```

**类比：** 就像手机的应用商店管理器，负责安装、卸载、更新APP。

### 3. 前台服务 (FrontService)
**位置：** `neo-bpsys-wpf/Services/FrontService.cs`
**接口：** `neo-bpsys-wpf.Core/Abstractions/Services/IFrontService.cs`

**作用：** 管理前台窗口和插件控件

**主要功能：**
- 🪟 管理多个前台窗口
- ➕ 添加插件控件到前台
- ➖ 移除插件控件
- 💾 保存/恢复控件位置
- 🎨 控制窗口显示/隐藏

**关键方法：**
```csharp
- AddPluginOverlayControl()    // 添加插件控件
- RemovePluginOverlayControl()  // 移除插件控件
- ShowWindow()                  // 显示窗口
- HideWindow()                  // 隐藏窗口
```

**类比：** 就像桌面窗口管理器，负责管理所有窗口和小部件的位置。

### 4. 设置服务 (SettingsHostService)
**位置：** `neo-bpsys-wpf/Services/SettingsHostService.cs`

**作用：** 管理应用配置

**主要功能：**
- 📖 读取配置文件
- 💾 保存配置文件
- 📝 管理禁用插件列表
- 🔔 配置变更通知

**配置文件位置：** `%AppData%/neo-bpsys-wpf/settings.json`

### 5. 插件上下文 (IPluginContext)
**位置：** `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs`

**作用：** 为插件提供访问主程序资源的入口

**提供的资源：**
```csharp
- Services     // 服务容器，可获取任何已注册的服务
- Host         // 应用主机
- Application  // WPF应用对象
- MainWindow   // 主窗口
```

**类比：** 就像给插件一个"通行证"，可以访问主程序的各种资源。

---

## 工作流程

### 启动流程

```
1. 应用启动
   ↓
2. 扫描 Plugins 目录
   ↓
3. 发现所有 DLL 中的 IPlugin 实现
   ↓
4. 过滤掉禁用的插件
   ↓
5. 构建宿主 (Host)
   ├─ 注册系统服务
   └─ 调用插件 ConfigureServices()
   ↓
6. 启动宿主
   ↓
7. 初始化插件
   └─ 调用插件 Initialize()
   ↓
8. 构建UI菜单
   ├─ 调用插件 GetMenuItems()
   └─ 调用插件 GetFooterItems()
   ↓
9. 添加前台控件
   └─ 调用插件 GetOverlayControls()
   ↓
10. 应用运行中
```

**关键代码位置：**
- 发现插件：`neo-bpsys-wpf/App.xaml.cs:39-40`
- 服务注册：`neo-bpsys-wpf/App.xaml.cs:206`
- 插件初始化：`neo-bpsys-wpf/App.xaml.cs:298`

### 热加载流程

```
用户点击"启用插件"
   ↓
1. 从缓存的类型创建插件实例
   ↓
2. 调用 ConfigureServices() (已跳过，因为Host已构建)
   ↓
3. 调用 Initialize()
   ↓
4. 添加菜单项到UI
   ↓
5. 添加前台控件
   ↓
6. 从禁用列表移除
   ↓
7. 保存配置
   ↓
8. 触发 PluginsChanged 事件
```

**代码位置：** `neo-bpsys-wpf/Extensions/PluginManager.cs:104-205`

### 热卸载流程

```
用户点击"禁用插件"
   ↓
1. 调用插件的 Dispose()
   ↓
2. 从前台服务移除控件
   ↓
3. 从菜单移除项
   ↓
4. 从内存移除实例
   ↓
5. 添加到禁用列表
   ↓
6. 保存配置
   ↓
7. 触发 PluginsChanged 事件
```

**代码位置：** `neo-bpsys-wpf/Extensions/PluginManager.cs:268-339`

---

## 数据流

### 插件发现

```
Plugins 目录
    ↓
扫描所有 *.dll 文件
    ↓
使用反射查找 IPlugin 实现
    ↓
检查禁用列表
    ↓
创建插件实例
    ↓
缓存类型信息
```

### 服务注册

```
主程序创建 ServiceCollection
    ↓
注册系统服务
    ↓
调用每个插件的 ConfigureServices()
    ↓
插件注册自己的服务
    ↓
构建 ServiceProvider
```

### UI集成

```
主窗口 ViewModel 初始化
    ↓
调用 PluginManager.GetAllMenuItems()
    ↓
遍历所有插件
    ↓
调用 plugin.GetMenuItems()
    ↓
收集所有菜单项
    ↓
构建导航菜单
```

---

## 关键设计模式

### 1. 依赖注入 (DI)
所有服务通过 DI 容器管理，插件可以注册和获取服务。

### 2. 事件驱动
使用事件通知UI插件状态变化：
```csharp
event EventHandler? PluginsChanged;
```

### 3. 工厂模式
前台控件使用工厂方法创建：
```csharp
ControlFactory = () => new MyControl()
```

### 4. 策略模式
不同类型的窗口可以有不同的控件管理策略。

---

## 扩展点

系统提供的扩展点：

| 扩展点 | 接口方法 | 作用 |
|-------|---------|------|
| 服务注册 | `ConfigureServices()` | 注册自定义服务 |
| 初始化 | `Initialize()` | 执行初始化逻辑 |
| 菜单 | `GetMenuItems()` | 添加菜单项 |
| 底部菜单 | `GetFooterItems()` | 添加底部菜单 |
| 前台控件 | `GetOverlayControls()` | 添加前台UI |
| 清理 | `Dispose()` | 清理资源 |

---

## 配置文件

### settings.json
```json
{
  "DisabledPlugins": [
    "bpsys.some.plugin",
    "another.disabled.plugin"
  ],
  // ... 其他设置
}
```

### front-positions.json
```json
{
  "PluginOverlayWindow_BaseCanvas": {
    "MyPlugin_Timer": {
      "Left": 100,
      "Top": 100,
      "Width": 200,
      "Height": 80
    }
  }
}
```

---

## 线程安全

- ✅ UI操作必须在UI线程执行
- ✅ 插件初始化在UI线程
- ✅ 服务容器是线程安全的
- ⚠️ 插件需自己处理多线程场景

---

## 性能考虑

1. **延迟加载** - 插件只在需要时初始化
2. **缓存类型** - 类型信息被缓存以提高热加载速度
3. **按需构建** - 菜单和控件按需创建

---

## 下一步

- 📖 [启动流程详解](startup-flow.md)
- 🔧 [插件管理器详解](plugin-manager.md)
- 🎨 [UI集成详解](ui-integration.md)
- ⚙️ [设置与配置](settings-config.md)

