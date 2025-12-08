using System;
using neo_bpsys_wpf.Core.Abstractions.Plugins;
using neo_bpsys_wpf.Core.Abstractions.Services;

namespace neo_bpsys_wpf.Core.Models;

public class PluginContext : IPluginContext
{
    public PluginContext(IServiceProvider services, IUiExtensionService ui)
    {
        Services = services;
        Ui = ui;
    }

    public IServiceProvider Services { get; }
    public IUiExtensionService Ui { get; }
}
