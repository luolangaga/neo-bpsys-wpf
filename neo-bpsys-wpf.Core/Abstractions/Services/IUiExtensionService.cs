using System.Windows;
using Wpf.Ui.Controls;

namespace neo_bpsys_wpf.Core.Abstractions.Services;

public interface IUiExtensionService
{
    void AddMenuItem(NavigationViewItem item);
    void AddFooterMenuItem(NavigationViewItem item);
    void AddResourceDictionary(ResourceDictionary dictionary);
}
