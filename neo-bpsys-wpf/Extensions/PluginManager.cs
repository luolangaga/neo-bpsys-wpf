using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using neo_bpsys_wpf.Core.Abstractions.Extensions;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;
using neo_bpsys_wpf.Core;
using System.Text.Json;

namespace neo_bpsys_wpf.Extensions;

public class PluginManager
{
    private readonly List<IPlugin> _plugins = new();
    private readonly List<PluginMetadata> _metadatas = new();
    public IReadOnlyList<PluginMetadata> Metadatas => _metadatas;
    private readonly List<PluginNavigationItem> _menuNavItems = new();
    private readonly List<PluginNavigationItem> _footerNavItems = new();
    public IReadOnlyList<PluginNavigationItem> MenuNavItems => _menuNavItems;
    public IReadOnlyList<PluginNavigationItem> FooterNavItems => _footerNavItems;

    private string? _lastPluginDir;
    private IHost? _host;
    private Application? _app;
    private Window? _mainWindow;
    public event EventHandler? PluginsChanged;

    public void Discover(string pluginDirectory)
    {
        _plugins.Clear();
        _metadatas.Clear();
        _menuNavItems.Clear();
        _footerNavItems.Clear();
        _lastPluginDir = pluginDirectory;
        if (!Directory.Exists(pluginDirectory)) return;

        var disabled = LoadDisabledPluginIds();

        foreach (var dll in Directory.EnumerateFiles(pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
                foreach (var t in asm.GetTypes())
                {
                    if (t.IsAbstract || t.IsInterface) continue;
                    if (!typeof(IPlugin).IsAssignableFrom(t)) continue;
                    if (Activator.CreateInstance(t) is IPlugin plugin)
                    {
                        var id = plugin.Metadata.Id ?? string.Empty;
                        if (disabled.Contains(id))
                        {
                            continue;
                        }
                        _plugins.Add(plugin);
                        _metadatas.Add(plugin.Metadata);
                        try
                        {
                            var menus = plugin.GetMenuItems();
                            if (menus != null) _menuNavItems.AddRange(menus);
                        }
                        catch { }
                        try
                        {
                            var footers = plugin.GetFooterItems();
                            if (footers != null) _footerNavItems.AddRange(footers);
                        }
                        catch { }
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        PluginsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        foreach (var p in _plugins)
        {
            try { p.ConfigureServices(services); } catch { }
        }
    }

    public void Initialize(IHost host, Application app, Window? mainWindow = null)
    {
        _host = host;
        _app = app;
        _mainWindow = mainWindow;
        var ctx = new PluginContext(host.Services, host, app, mainWindow);
        foreach (var p in _plugins)
        {
            try { p.Initialize(ctx); } catch { }
        }
        PluginsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Reload()
    {
        if (string.IsNullOrEmpty(_lastPluginDir)) return;
        Discover(_lastPluginDir);
        if (_host != null && _app != null)
        {
            Initialize(_host, _app, _mainWindow);
        }
    }

    private static HashSet<string> LoadDisabledPluginIds()
    {
        try
        {
            if (!File.Exists(AppConstants.ConfigFilePath)) return new HashSet<string>();
            using var doc = JsonDocument.Parse(File.ReadAllText(AppConstants.ConfigFilePath));
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return new HashSet<string>();
            if (!doc.RootElement.TryGetProperty("DisabledPlugins", out var arr)) return new HashSet<string>();
            if (arr.ValueKind != JsonValueKind.Array) return new HashSet<string>();
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in arr.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var id = item.GetString();
                    if (!string.IsNullOrWhiteSpace(id)) set.Add(id);
                }
            }
            return set;
        }
        catch
        {
            return new HashSet<string>();
        }
    }

    private record PluginContext(IServiceProvider Services, IHost Host, Application Application, Window? MainWindow) : IPluginContext;
}
