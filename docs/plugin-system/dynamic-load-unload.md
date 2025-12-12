# 插件动态加载与卸载

## 概述

插件系统现已支持动态加载和卸载插件，无需重新启动应用程序即可启用或禁用插件。

## 功能特性

### 1. 动态卸载插件

当用户禁用插件时：
- 自动调用插件的 `Dispose()` 方法清理资源
- 从前台窗口移除插件的覆盖控件
- 从菜单和导航栏移除插件添加的项
- 从内存中卸载插件实例
- 更新配置文件，下次启动时不会加载该插件

### 2. 动态加载插件

当用户启用插件时：
- 创建插件的新实例
- 调用插件的 `Initialize()` 方法进行初始化
- 将插件的覆盖控件添加到前台窗口
- 将插件的菜单项添加到导航栏
- 从配置文件的禁用列表中移除，下次启动时会自动加载

### 3. 资源清理机制

插件可以通过实现 `Dispose()` 方法来清理资源：

```csharp
public class MyPlugin : IPlugin
{
    private Timer? _timer;
    private EventHandler? _eventHandler;
    
    public void Initialize(IPluginContext context)
    {
        _timer = new Timer();
        _timer.Start();
        
        // 订阅事件
        SomeService.SomeEvent += _eventHandler;
    }
    
    public void Dispose()
    {
        // 清理定时器
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }
        
        // 取消事件订阅
        if (_eventHandler != null)
        {
            SomeService.SomeEvent -= _eventHandler;
            _eventHandler = null;
        }
        
        // 清理其他资源
    }
}
```

## 实现原理

### PluginManager 核心改进

1. **插件实例管理**
   - 使用 `Dictionary<string, IPlugin>` 管理已加载的插件实例
   - 使用 `Dictionary<string, Type>` 缓存插件类型信息

2. **UnloadPlugin 方法**
   - 调用插件的 `Dispose()` 方法
   - 从 `IFrontService` 移除插件控件
   - 从集合中移除插件的所有数据
   - 触发 `PluginsChanged` 事件更新 UI

3. **LoadPlugin 方法**
   - 检查插件是否已加载，避免重复加载
   - 从缓存的类型信息创建新实例
   - 调用 `Initialize()` 初始化插件
   - 通过 `IFrontService` 添加插件控件
   - 触发 `PluginsChanged` 事件更新 UI

### ExtensionPageViewModel 改进

**禁用插件流程：**
```csharp
[RelayCommand]
private void DisablePlugin(string id)
{
    // 1. 动态卸载插件
    _pluginManager.UnloadPlugin(id);
    
    // 2. 添加到禁用列表
    disabled.Add(id);
    _settingsHostService.SaveConfig();
}
```

**启用插件流程：**
```csharp
[RelayCommand]
private void EnablePlugin(string id)
{
    // 1. 从禁用列表移除
    disabled.Remove(id);
    _settingsHostService.SaveConfig();
    
    // 2. 动态加载插件
    _pluginManager.LoadPlugin(id);
}
```

## 使用场景

### 场景 1：临时禁用插件

用户在比赛期间发现某个插件影响性能，可以立即禁用该插件，无需重启应用。

### 场景 2：测试新插件

开发者可以启用新插件进行测试，发现问题后立即禁用，无需反复重启应用。

### 场景 3：资源管理

插件被卸载时会自动清理占用的资源（如定时器、事件订阅、网络连接等），避免内存泄漏。

## 注意事项

### 插件开发者

1. **实现 Dispose 方法**
   - 虽然 `Dispose()` 方法有默认空实现，但强烈建议实现它
   - 清理所有订阅的事件，避免内存泄漏
   - 释放占用的资源（定时器、文件句柄、网络连接等）

2. **避免静态状态**
   - 插件卸载后静态变量不会被清理
   - 尽量使用实例变量而非静态变量

3. **UI 控件清理**
   - `PluginManager` 会自动从前台窗口移除插件的覆盖控件
   - 如果控件内部有订阅事件或其他资源，需要在控件中处理清理

### 服务注册限制

由于 .NET 依赖注入容器的限制，插件通过 `ConfigureServices()` 注册的服务在应用生命周期内持续存在，无法在插件卸载时自动移除。因此：

- **建议**: 将需要动态卸载的资源放在插件实例中，而非注册为服务
- **服务注册**: 仅用于应用级别的单例服务或共享服务

## API 参考

### IPlugin 接口

```csharp
public interface IPlugin
{
    PluginMetadata Metadata { get; }
    void ConfigureServices(IServiceCollection services);
    void Initialize(IPluginContext context);
    IEnumerable<PluginNavigationItem> GetMenuItems();
    IEnumerable<PluginNavigationItem> GetFooterItems();
    IEnumerable<PluginOverlayDescriptor> GetOverlayControls();
    
    /// <summary>
    /// 释放插件资源，在插件被禁用或卸载时调用
    /// </summary>
    void Dispose() { }
}
```

### PluginManager 方法

```csharp
/// <summary>
/// 动态卸载单个插件
/// </summary>
/// <param name="pluginId">插件 ID</param>
/// <returns>是否成功卸载</returns>
public bool UnloadPlugin(string pluginId)

/// <summary>
/// 动态加载单个插件
/// </summary>
/// <param name="pluginId">插件 ID</param>
/// <returns>是否成功加载</returns>
public bool LoadPlugin(string pluginId)
```

## 相关文件

- `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs` - 插件接口定义
- `neo-bpsys-wpf/Extensions/PluginManager.cs` - 插件管理器实现
- `neo-bpsys-wpf/ViewModels/Pages/ExtensionPageViewModel.cs` - 扩展页面视图模型
- `plugins/OverlaySample/OverlaySamplePlugin.cs` - 插件示例

## 更新日志

- **2025-12-12**: 实现插件动态加载和卸载功能
  - 为 `IPlugin` 接口添加 `Dispose()` 方法
  - 在 `PluginManager` 中实现 `LoadPlugin()` 和 `UnloadPlugin()` 方法
  - 更新 `ExtensionPageViewModel` 以支持动态操作
