using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using neo_bpsys_wpf.Core.Abstractions.Plugins;
using neo_bpsys_wpf.Core.Abstractions.Services;
using neo_bpsys_wpf.Core.Models;
using Wpf.Ui.Controls;

namespace neo_bpsys_wpf.SamplePlugin;

public class HelloPlugin : IPlugin
{
    public PluginMetadata Metadata => new()
    {
        Id = "hello.plugin",
        Name = "Hello 插件",
        Description = "演示如何通过插件扩展菜单并添加页面",
        Version = "1.0.0",
        Author = "Trae"
    };

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<HelloPage>();
    }

    public void Initialize(IPluginContext context)
    {
        var logger = context.Services.GetService<ILogger<HelloPlugin>>();
        logger?.LogInformation("初始化插件 {Name}", Metadata.Name);
        var item = new NavigationViewItem("Hello 插件页面", SymbolRegular.AppsAddIn24, typeof(HelloPage));
        context.Ui.AddFooterMenuItem(item);
    }
}
