using System;
using neo_bpsys_wpf.Core.Abstractions.Services;

namespace neo_bpsys_wpf.Core.Abstractions.Plugins;

public interface IPluginContext
{
    IServiceProvider Services { get; }
    IUiExtensionService Ui { get; }
}
