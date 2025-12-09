using System.Threading.Tasks;

namespace neo_bpsys_wpf.Core.Abstractions.Services;

public interface IAppRestartService
{
    /// <summary>
    /// 重启应用程序
    /// </summary>
    void RestartApplication();
}