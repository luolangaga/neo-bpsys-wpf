using Microsoft.Extensions.DependencyInjection;
using neo_bpsys_wpf.Core.Abstractions.Extensions;
using neo_bpsys_wpf.Core.Abstractions.Services;

namespace Bpsys.Plugin.OCR;

public class OcrPlugin : IPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "bpsys.ocr",
        Name = "OCR 识别插件",
        Version = "1.0.0",
        Description = "提供 OCR 模型管理与下载",
        Author = "罗澜"
    };

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IOcrModelService, Services.OcrModelService>();
        services.AddSingleton<ViewModels.Pages.OcrHelperPageViewModel>();
        services.AddSingleton<Views.Pages.OcrHelperPage>(sp => new Views.Pages.OcrHelperPage()
        {
            DataContext = sp.GetRequiredService<ViewModels.Pages.OcrHelperPageViewModel>()
        });
    }

    public void Initialize(IPluginContext context)
    {
        // 在此处可扩展 OCR 相关的 UI 或快捷操作
    }

    public IEnumerable<PluginNavigationItem> GetMenuItems()
    {
        return new[]
        {
            new PluginNavigationItem
            {
                Title = "识别助手",
                PageType = typeof(Views.Pages.OcrHelperPage)
            }
        };
    }

    public IEnumerable<PluginNavigationItem> GetFooterItems()
    {
        return Array.Empty<PluginNavigationItem>();
    }
}
