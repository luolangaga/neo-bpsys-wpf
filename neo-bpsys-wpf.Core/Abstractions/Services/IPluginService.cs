using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using neo_bpsys_wpf.Core.Abstractions.Plugins;

namespace neo_bpsys_wpf.Core.Abstractions.Services;

public interface IPluginService
{
    IReadOnlyList<IPlugin> Plugins { get; }
    void Discover(string? dir = null);
    void ConfigureServices(IServiceCollection services);
    void Initialize(IPluginContext context);
    void SetEnabled(string id, bool enabled);
    bool IsEnabled(string id);
}
