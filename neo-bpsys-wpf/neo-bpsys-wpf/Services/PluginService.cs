using System.Reflection;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using neo_bpsys_wpf.Core.Abstractions.Plugins;
using neo_bpsys_wpf.Core.Abstractions.Services;

namespace neo_bpsys_wpf.Services;

public class PluginService : IPluginService
{
    private readonly List<IPlugin> _plugins = [];
    private readonly Dictionary<string, bool> _enabled = new();
    private readonly IServiceProvider _serviceProvider;

    public PluginService(IServiceProvider serviceProvider, IEnumerable<IPlugin>? preloaded)
    {
        _serviceProvider = serviceProvider;
        if (preloaded != null)
        {
            foreach (var p in preloaded)
            {
                _plugins.Add(p);
                _enabled[p.Metadata.Id] = true;
            }
        }
    }

    public IReadOnlyList<IPlugin> Plugins => _plugins;

    public void Discover(string? dir = null)
    {
        var baseDir = dir ?? AppDomain.CurrentDomain.BaseDirectory;
        var pluginDir = Path.Combine(baseDir, "Plugins");
        if (!Directory.Exists(pluginDir)) return;
        foreach (var file in Directory.EnumerateFiles(pluginDir, "*.dll"))
        {
            var asm = Assembly.LoadFrom(file);
            foreach (var type in asm.GetTypes())
            {
                if (!typeof(IPlugin).IsAssignableFrom(type) || type.IsAbstract) continue;
                var inst = (IPlugin)Activator.CreateInstance(type)!;
                if (_plugins.Any(x => x.Metadata.Id == inst.Metadata.Id)) continue;
                _plugins.Add(inst);
                _enabled[inst.Metadata.Id] = true;
            }
        }
    }

    public void ConfigureServices(IServiceCollection services)
    {
        foreach (var p in _plugins)
        {
            if (!_enabled.TryGetValue(p.Metadata.Id, out var en) || !en) continue;
            p.ConfigureServices(services);
        }
    }

    public void Initialize(IPluginContext context)
    {
        foreach (var p in _plugins)
        {
            if (!_enabled.TryGetValue(p.Metadata.Id, out var en) || !en) continue;
            p.Initialize(context);
        }
    }

    public void SetEnabled(string id, bool enabled)
    {
        _enabled[id] = enabled;
    }

    public bool IsEnabled(string id)
    {
        return _enabled.TryGetValue(id, out var en) && en;
    }
}
