using neo_bpsys_wpf.ViewModels.Pages;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace neo_bpsys_wpf.Views.Pages;

/// <summary>
/// PluginControlManagePage.xaml 的交互逻辑
/// </summary>
public partial class PluginControlManagePage : Page
{
    public PluginControlManagePageViewModel ViewModel { get; }

    public PluginControlManagePage(PluginControlManagePageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        
        InitializeComponent();
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // 页面加载时刷新控件列表
        ViewModel.RefreshControlsCommand.Execute(null);
    }
}
