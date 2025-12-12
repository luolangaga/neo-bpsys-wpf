using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using neo_bpsys_wpf.Core.Abstractions.Extensions;
using neo_bpsys_wpf.Core.Abstractions.Services;
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
    private readonly List<PluginOverlayDescriptor> _overlayDescriptors = new();
    public IReadOnlyList<PluginNavigationItem> MenuNavItems => _menuNavItems;
    public IReadOnlyList<PluginNavigationItem> FooterNavItems => _footerNavItems;
    public IReadOnlyList<PluginOverlayDescriptor> OverlayDescriptors => _overlayDescriptors;

    // 使用字典管理插件实例，方便动态加载/卸载
    private readonly Dictionary<string, IPlugin> _pluginInstances = new();
    private readonly Dictionary<string, Type> _pluginTypes = new();
    
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
        _overlayDescriptors.Clear();
        _pluginTypes.Clear();
        
        _lastPluginDir = pluginDirectory;
        if (!Directory.Exists(pluginDirectory)) return;

        var disabled = LoadDisabledPluginIds();

        // 临时诊断日志：记录发现插件与禁用列表，方便排查运行时加载问题
        try
        {
            var outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginsFound.log");
            File.AppendAllText(outDir, $"\n--- Discover at {DateTime.Now:O} ---\n");
            // DisabledPlugins feature removed — no longer log or use disabled list.
        }
        catch { }

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
                        try
                        {
                            var outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginsFound.log");
                            File.AppendAllText(outDir, $"Found plugin: Id={id}, Name={plugin.Metadata.Name}\n");
                        }
                        catch { }
                        
                        // 保存插件类型用于动态加载
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            _pluginTypes[id] = t;
                        }
                        
                        if (disabled.Contains(id))
                        {
                            try
                            {
                                var outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginsFound.log");
                                File.AppendAllText(outDir, $"Plugin {id} is disabled in config, skipping.\n");
                            }
                            catch { }
                            continue;
                        }
                        
                        _plugins.Add(plugin);
                        _pluginInstances[id] = plugin;
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
                        try
                        {
                            var overlays = plugin.GetOverlayControls();
                            if (overlays != null) _overlayDescriptors.AddRange(overlays);
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
        
        // 加载插件覆盖控件到前台窗口
        LoadPluginOverlayControls(host.Services);
        
        PluginsChanged?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// 加载插件覆盖控件到前台窗口
    /// </summary>
    private void LoadPluginOverlayControls(IServiceProvider services)
    {
        try
        {
            var frontService = services.GetService<IFrontService>();
            if (frontService == null) return;
            
            // 清除旧的插件控件
            frontService.ClearPluginOverlayControls();
            
            // 添加新的插件控件
            foreach (var descriptor in _overlayDescriptors)
            {
                try
                {
                    frontService.AddPluginOverlayControl(descriptor);
                }
                catch { }
            }
        }
        catch { }
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
    
    /// <summary>
    /// 动态卸载单个插件
    /// </summary>
    /// <param name="pluginId">插件 ID</param>
    /// <returns>是否成功卸载</returns>
    public bool UnloadPlugin(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId)) return false;
        if (!_pluginInstances.TryGetValue(pluginId, out var plugin)) return false;

        try
        {
            // 调用插件的 Dispose 方法清理资源
            plugin.Dispose();
            
            // 移除插件的覆盖控件
            if (_host?.Services != null)
            {
                var frontService = _host.Services.GetService<IFrontService>();
                if (frontService != null)
                {
                    var overlays = plugin.GetOverlayControls();
                    if (overlays != null)
                    {
                        foreach (var overlay in overlays)
                        {
                            try
                            {
                                frontService.RemovePluginOverlayControl(overlay.Id);
                            }
                            catch { }
                        }
                    }
                }
            }
            
            // 从集合中移除插件
            _plugins.Remove(plugin);
            _pluginInstances.Remove(pluginId);
            _metadatas.RemoveAll(m => m.Id == pluginId);
            
            // 移除菜单项
            var menus = plugin.GetMenuItems();
            if (menus != null)
            {
                foreach (var menu in menus)
                {
                    _menuNavItems.Remove(menu);
                }
            }
            
            // 移除底部导航项
            var footers = plugin.GetFooterItems();
            if (footers != null)
            {
                foreach (var footer in footers)
                {
                    _footerNavItems.Remove(footer);
                }
            }
            
            // 移除覆盖控件描述符
            var overlayDescs = plugin.GetOverlayControls();
            if (overlayDescs != null)
            {
                foreach (var desc in overlayDescs)
                {
                    _overlayDescriptors.Remove(desc);
                }
            }
            
            PluginsChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 动态加载单个插件
    /// </summary>
    /// <param name="pluginId">插件 ID</param>
    /// <returns>是否成功加载</returns>
    public bool LoadPlugin(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId)) return false;
        
        // 如果插件已经加载，不重复加载
        if (_pluginInstances.ContainsKey(pluginId)) return false;
        
        // 检查是否有该插件的类型信息
        if (!_pluginTypes.TryGetValue(pluginId, out var pluginType)) return false;

        try
        {
            // 创建插件实例
            if (Activator.CreateInstance(pluginType) is not IPlugin plugin) return false;
            
            // 添加到集合
            _plugins.Add(plugin);
            _pluginInstances[pluginId] = plugin;
            _metadatas.Add(plugin.Metadata);
            
            // 添加菜单项
            try
            {
                var menus = plugin.GetMenuItems();
                if (menus != null) _menuNavItems.AddRange(menus);
            }
            catch { }
            
            // 添加底部导航项
            try
            {
                var footers = plugin.GetFooterItems();
                if (footers != null) _footerNavItems.AddRange(footers);
            }
            catch { }
            
            // 添加覆盖控件描述符
            try
            {
                var overlays = plugin.GetOverlayControls();
                if (overlays != null) _overlayDescriptors.AddRange(overlays);
            }
            catch { }
            
            // 如果已经初始化过，立即初始化新插件
            if (_host != null && _app != null)
            {
                try
                {
                    var ctx = new PluginContext(_host.Services, _host, _app, _mainWindow);
                    plugin.Initialize(ctx);
                    
                    // 加载插件的覆盖控件
                    var frontService = _host.Services.GetService<IFrontService>();
                    if (frontService != null)
                    {
                        var overlays = plugin.GetOverlayControls();
                        if (overlays != null)
                        {
                            foreach (var descriptor in overlays)
                            {
                                try
                                {
                                    frontService.AddPluginOverlayControl(descriptor);
                                }
                                catch { }
                            }
                        }
                    }
                }
                catch { }
            }
            
            PluginsChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static HashSet<string> LoadDisabledPluginIds()
    {
        // DisabledPlugins feature has been removed — always enable all plugins.
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private record PluginContext(IServiceProvider Services, IHost Host, Application Application, Window? MainWindow) : IPluginContext;
}
