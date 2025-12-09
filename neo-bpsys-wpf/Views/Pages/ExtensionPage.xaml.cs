using System.Windows;
using System.Windows.Controls;

namespace neo_bpsys_wpf.Views.Pages;

/// <summary>
/// ExtensionPage.xaml 的交互逻辑
/// </summary>
public partial class ExtensionPage : Page
{
    public ExtensionPage()
    {
        InitializeComponent();
    }

    private void Border_ManipulationInertiaStarting(object sender, System.Windows.Input.ManipulationInertiaStartingEventArgs e)
    {

    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is neo_bpsys_wpf.ViewModels.Pages.ExtensionPageViewModel vm)
        {
            vm.RefreshMarketplaceCommand.Execute(null);
        }
    }
}
