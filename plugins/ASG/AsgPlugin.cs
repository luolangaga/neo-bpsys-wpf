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
    }

    public void Initialize(IPluginContext context)
    {
        var asgService = context.Services.GetRequiredService<IASGService>();
        var settingsHostService = context.Services.GetRequiredService<ISettingsHostService>();
        if (context.MainWindow == null) return;
        context.MainWindow.Loaded += async (s, e) =>
        {
            await Task.Delay(800);
            var settings = settingsHostService.Settings;
            if (!asgService.IsLoggedIn)
            {
                if (!string.IsNullOrWhiteSpace(settings.AsgEmail) && !string.IsNullOrWhiteSpace(settings.AsgPassword))
                {
                    var ok = await asgService.LoginAsync(settings.AsgEmail!, settings.AsgPassword!);
                    if (ok) return;
                }

                var emailTextBox = new TextBox
                {
                    Width = 240,
                    PlaceholderText = "邮箱",
                    Text = settings.AsgEmail ?? string.Empty
                };
                var passwordTextBox = new TextBox
                {
                    Width = 180,
                    Margin = new Thickness(10, 0, 0, 0),
                    PlaceholderText = "密码",
                    Text = settings.AsgPassword ?? string.Empty
                };
                var stackPanel = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
                stackPanel.Children.Add(emailTextBox);
                stackPanel.Children.Add(passwordTextBox);

                var messageBox = new MessageBox()
                {
                    Title = "登录提示",
                    Content = stackPanel,
                    PrimaryButtonText = "登录",
                    PrimaryButtonIcon = new SymbolIcon() { Symbol = SymbolRegular.ArrowImport24 },
                    CloseButtonIcon = new SymbolIcon() { Symbol = SymbolRegular.Dismiss24 },
                    CloseButtonText = "取消",
                    Owner = Application.Current.MainWindow,
                };
                var result = await messageBox.ShowDialogAsync();
                if (result == MessageBoxResult.Primary)
                {
                    var ok = await asgService.LoginAsync(emailTextBox.Text, passwordTextBox.Text);
                    if (ok)
                    {
                        settings.AsgEmail = emailTextBox.Text;
                        settings.AsgPassword = passwordTextBox.Text;
                        settingsHostService.SaveConfig();
                    }
                    else
                    {
                        var msg = context.Services.GetRequiredService<IMessageBoxService>();
                        await msg.ShowErrorAsync("登录失败，请稍后在设置中重试", "提示");
                    }
                }
            }
        };
    }

    public IEnumerable<PluginNavigationItem> GetMenuItems()
    {
        return Array.Empty<PluginNavigationItem>();
    }

    public IEnumerable<PluginNavigationItem> GetFooterItems()
    {
        return Array.Empty<PluginNavigationItem>();
    }
}
