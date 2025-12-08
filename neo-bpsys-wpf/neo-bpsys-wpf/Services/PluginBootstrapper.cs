using System.Reflection;
using System.IO;
using neo_bpsys_wpf.Core.Abstractions.Plugins;

namespace neo_bpsys_wpf.Services;

public static class PluginBootstrapper
{
    public static List<IPlugin> DiscoverPlugins(string? baseDir = null)
    {
        var list = new List<IPlugin>();
        var dir = baseDir ?? AppDomain.CurrentDomain.BaseDirectory;
        var pluginDir = Path.Combine(dir, "Plugins");
        if (!Directory.Exists(pluginDir)) return list;
        foreach (var file in Directory.EnumerateFiles(pluginDir, "*.dll"))
        {
            var asm = Assembly.LoadFrom(file);
            foreach (var type in asm.GetTypes())
            {
                if (!typeof(IPlugin).IsAssignableFrom(type) || type.IsAbstract) continue;
                var inst = (IPlugin)Activator.CreateInstance(type)!;
                list.Add(inst);
            }
        }
        return list;
    }
}
