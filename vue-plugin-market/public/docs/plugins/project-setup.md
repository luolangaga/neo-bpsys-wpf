# 项目与输出配置

让插件项目正确输出到主程序的 `Plugins` 目录，并启用 WPF 支持。

## `csproj` 模板

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

参考实现：`plugins/ASG/ASG.Plugin.csproj:1`。

## 注意事项

- 输出路径必须指向主程序 `bin/<配置>/net9.0-windows7.0/Plugins/`，应用启动时只扫描该目录：`neo-bpsys-wpf/App.xaml.cs:39`。
- 若需要 UI 控件（如弹窗、页面），请启用 `UseWPF`。
- 通过 `ProjectReference` 引用 Core，以使用 `IPlugin`、`PluginMetadata` 等接口：`neo-bpsys-wpf.Core/Abstractions/Extensions/IPlugin.cs:11`。
- 若出现 “Data at the root level is invalid” 构建错误，检查文件是否带有 BOM 或无效头部字符，重新保存为 UTF-8。

