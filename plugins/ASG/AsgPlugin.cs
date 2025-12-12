using Microsoft.Extensions.DependencyInjection;
using neo_bpsys_wpf.Core.Abstractions.Extensions;
using neo_bpsys_wpf.Core.Abstractions.Services;
using System.Windows;
using Wpf.Ui.Controls;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;

namespace Bpsys.Plugin.ASG;

public class AsgPlugin : IPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "bpsys.asg",
        Name = "ASG 功能插件",
        Version = "1.0.0",
        Description = "提供 ASG API 集成与登录支持",
        Author = "neo-bpsys-wpf"
    };

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IASGService, Services.ASGService>();
        services.AddSingleton<ViewModels.Pages.AsgHelperPageViewModel>();
        services.AddSingleton<Views.Pages.AsgHelperPage>(sp => new Views.Pages.AsgHelperPage
        {
            DataContext = sp.GetRequiredService<ViewModels.Pages.AsgHelperPageViewModel>()
        });
    }

    public void Initialize(IPluginContext context)
    {
        var asgService = context.Services.GetRequiredService<IASGService>();
        var settingsHostService = context.Services.GetRequiredService<ISettingsHostService>();
        if (context.MainWindow == null) return;
        context.MainWindow.Loaded += async (s, e) =>
        {
            await Task.Delay(500);
            var settings = settingsHostService.Settings;
            if (asgService.IsLoggedIn) return;
            if (string.IsNullOrWhiteSpace(settings.AsgEmail) || string.IsNullOrWhiteSpace(settings.AsgPassword)) return;
            try
            {
                await asgService.LoginAsync(settings.AsgEmail!, settings.AsgPassword!);
            }
            catch
            {
                // ignore auto login failures and let the helper页 handle manual login
            }
        };
    }

    public IEnumerable<PluginNavigationItem> GetMenuItems()
    {
        return new[]
        {
            new PluginNavigationItem
            {
                Title = "赛事同步",
                PageType = typeof(Views.Pages.AsgHelperPage)
            }
        };
    }

    public IEnumerable<PluginNavigationItem> GetFooterItems()
    {
        return Array.Empty<PluginNavigationItem>();
    }
}
