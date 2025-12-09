# 插件开发新手指南
新手建议按“分章索引”逐步学习与实践：
- 插件开发索引：`docs/plugins/index.md`
- 快速上手：`docs/plugins/getting-started.md`
- 项目与输出配置：`docs/plugins/project-setup.md`
- 核心接口与生命周期：`docs/plugins/interfaces.md`
- 菜单与页面注入：`docs/plugins/ui-navigation.md`
- 初始化场景与交互示例：`docs/plugins/init-examples.md`
- 热加载与禁用：`docs/plugins/hot-reload-disable.md`
- 测试与调试：`docs/plugins/testing-debugging.md`
- 最佳实践：`docs/plugins/best-practices.md`
- FAQ 常见问题：`docs/plugins/faq.md`

本指南面向第一次为本项目编写插件的开发者，帮助你用最短时间上手、构建并运行一个可加载的插件。插件系统基于 .NET/WPF 与依赖注入实现，支持运行时热加载与禁用。

## 前置条件

- .NET SDK：`net9.0-windows7.0`
- IDE：Visual Studio 2022 或 Rider，亦可使用命令行
- 目标平台：Windows 10/11

## 插件目录与输出

- 插件以独立 `Class Library` 项目形式存在，建议置于仓库 `plugins/你的插件名` 目录。
- 构建时需要将插件输出到主程序的 `Plugins` 目录，方便应用在启动时扫描与加载。

示例 `csproj`（关键项：目标框架、引用 Core、启用 WPF、输出路径）：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-windows7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <Deterministic>false</Deterministic>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <AssemblyName>Bpsys.Plugin.Sample</AssemblyName>
    <RootNamespace>Bpsys.Plugin.Sample</RootNamespace>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <OutputPath>$(MSBuildThisFileDirectory)..\..\neo-bpsys-wpf\bin\$(Configuration)\net9.0-windows7.0\Plugins\</OutputPath>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\neo-bpsys-wpf.Core\neo-bpsys-wpf.Core.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="WPF-UI" Version="4.0.3" />
  </ItemGroup>
</Project>
```

参考：`plugins/ASG/ASG.Plugin.csproj:12` 启用了 `UseWPF`；`plugins/ASG/ASG.Plugin.csproj:10` 指定了输出路径到主程序 `Plugins` 目录。

## 核心接口与生命周期

插件需实现 `IPlugin` 接口，完成元数据、服务注册、初始化与菜单/页签注入：

```csharp
public class SamplePlugin : IPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "bpsys.sample",
        Name = "示例插件",
        Version = "1.0.0",
        Description = "演示如何开发一个插件",
        Author = "you"
    };

    public void ConfigureServices(IServiceCollection services)
    {
        // 在此注册你的服务到 DI 容器
    }

    public void Initialize(IPluginContext context)
    {
        // 运行期初始化：订阅事件、访问 MainWindow、读取 Settings 等
    }

    public IEnumerable<PluginNavigationItem> GetMenuItems()
    {
        // 返回要挂到左侧菜单的页面
        return Array.Empty<PluginNavigationItem>();
    }

    public IEnumerable<PluginNavigationItem> GetFooterItems()
    {
        // 返回底部菜单项（如外链/关于）
        return Array.Empty<PluginNavigationItem>();
    }
}
```

接口定义位置：

- `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:11`（`PluginMetadata`）
- `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:12`（`ConfigureServices`）
- `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:13`（`Initialize`）
- `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:14`（`GetMenuItems`）
- `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:15`（`GetFooterItems`）
- `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:20`（`IPluginContext.Services`）
- `neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:23`（`IPluginContext.MainWindow`）

## 菜单与页面注入

插件可以把自己的页面挂到主界面菜单。参考 OCR 插件：

- 注册页面及其 ViewModel 到 DI：`plugins/OCR/OcrPlugin.cs:18`
- 返回菜单项并指定页面类型：`plugins/OCR/OcrPlugin.cs:33`、`plugins/OCR/OcrPlugin.cs:39`

```csharp
public IEnumerable<PluginNavigationItem> GetMenuItems()
{
    return new[]
    {
        new PluginNavigationItem
        {
            Title = "识别助手",
            PageType = typeof(Views.Pages.OcrHelperPage)
        }
    };
}
```

主程序在启动时会构建菜单，并监听插件变化以重建菜单：`neo-bpsys-wpf/ViewModels/Windows/MainWindowViewModel.cs:114`。

## 初始化与主窗体交互示例

如果需要在主窗体加载后做动作（例如弹窗登录），可在 `Initialize` 中订阅 `MainWindow.Loaded`。参考 ASG 插件：

- 初始化入口：`plugins/ASG/AsgPlugin.cs:25`
- 订阅主窗体 Loaded：`plugins/ASG/AsgPlugin.cs:30`
- 弹窗并处理登录保存：`plugins/ASG/AsgPlugin.cs:59`、`plugins/ASG/AsgPlugin.cs:75`

## 应用启动时的插件发现与初始化

- 启动阶段发现插件：`neo-bpsys-wpf/App.xaml.cs:39`、`neo-bpsys-wpf/App.xaml.cs:40`
- 插件服务注册在宿主构建末尾：`neo-bpsys-wpf/App.xaml.cs:206`
- 宿主启动后初始化插件：`neo-bpsys-wpf/App.xaml.cs:298`

核心管理器：

- 发现并过滤禁用插件：`neo-bpsys-wpf/Extensions/PluginManager.cs:30`、`neo-bpsys-wpf/Extensions/PluginManager.cs:53`
- 初始化并触发变更事件：`neo-bpsys-wpf/Extensions/PluginManager.cs:91`、`neo-bpsys-wpf/Extensions/PluginManager.cs:101`
- 热加载入口：`neo-bpsys-wpf/Extensions/PluginManager.cs:104`

## 热加载与禁用

- 配置文件路径：`neo-bpsys-wpf.Core/AppConstants.cs:29`
- 禁用列表字段：`neo-bpsys-wpf.Core/Models/Settings.cs:20`（`DisabledPlugins`）
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
