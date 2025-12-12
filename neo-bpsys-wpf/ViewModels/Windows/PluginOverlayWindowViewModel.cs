using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using neo_bpsys_wpf.Core.Abstractions.ViewModels;
using neo_bpsys_wpf.Core.Enums;
using neo_bpsys_wpf.Core.Messages;

namespace neo_bpsys_wpf.ViewModels.Windows;

public partial class PluginOverlayWindowViewModel : ViewModelBase, IRecipient<DesignModeChangedMessage>
{
    public PluginOverlayWindowViewModel()
    {
        // Decorative constructor, used in conjunction with IsDesignTimeCreatable=True
    }

    [ObservableProperty] private bool _isDesignMode;

    public void Receive(DesignModeChangedMessage message)
    {
        if (message.FrontWindowType == FrontWindowType.PluginOverlayWindow && IsDesignMode != message.IsDesignMode)
            IsDesignMode = message.IsDesignMode;
    }
}
