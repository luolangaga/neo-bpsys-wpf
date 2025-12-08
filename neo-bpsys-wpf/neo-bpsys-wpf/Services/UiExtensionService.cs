using System.Windows;
using neo_bpsys_wpf.Core.Abstractions.Services;
using neo_bpsys_wpf.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace neo_bpsys_wpf.Services;

public class UiExtensionService : IUiExtensionService
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    public UiExtensionService(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }

    public void AddMenuItem(NavigationViewItem item)
    {
        _mainWindowViewModel.MenuItems.Add(item);
    }

    public void AddFooterMenuItem(NavigationViewItem item)
    {
        _mainWindowViewModel.FooterMenuItems.Add(item);
    }

    public void AddResourceDictionary(ResourceDictionary dictionary)
    {
        Application.Current.Resources.MergedDictionaries.Add(dictionary);
    }
}
