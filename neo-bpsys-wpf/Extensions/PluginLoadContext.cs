using System.Reflection;
using System.Runtime.Loader;

namespace neo_bpsys_wpf.Extensions;

/// <summary>
/// Collectible load context per plugin to enable unloading and shadow-copy loading.
/// </summary>
internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string mainAssemblyPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (!string.IsNullOrEmpty(assemblyPath))
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var unmanagedPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (!string.IsNullOrEmpty(unmanagedPath))
        {
            return LoadUnmanagedDllFromPath(unmanagedPath);
        }

        return base.LoadUnmanagedDll(unmanagedDllName);
    }
}
