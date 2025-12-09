using neo_bpsys_wpf.Core.Abstractions.Services;

namespace neo_bpsys_wpf.Services;

public class AppRestartService : IAppRestartService
{
    public void RestartApplication()
    {
        App.Restart();
    }
}
