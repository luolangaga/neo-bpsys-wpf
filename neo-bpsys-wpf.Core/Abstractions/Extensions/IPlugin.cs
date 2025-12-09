using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Windows;

namespace neo_bpsys_wpf.Core.Abstractions.Extensions;

public interface IPlugin
{
    PluginMetadata Metadata { get; }
    void ConfigureServices(IServiceCollection services);
    void Initialize(IPluginContext context);
    IEnumerable<PluginNavigationItem> GetMenuItems();
    IEnumerable<PluginNavigationItem> GetFooterItems();
}

public interface IPluginContext
{
    IServiceProvider Services { get; }
    IHost Host { get; }
    Application Application { get; }
    Window? MainWindow { get; }
}

public class PluginMetadata
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = "1.0.0";
    public string? Description { get; init; }
    public string? Author { get; init; }
}

public class PluginNavigationItem
{
    public string Title { get; init; } = string.Empty;
    public Type PageType { get; init; } = typeof(object);
}
