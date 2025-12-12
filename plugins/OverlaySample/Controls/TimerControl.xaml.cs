using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Bpsys.Plugin.OverlaySample.Controls;

/// <summary>
/// 倒计时器控件 - 展示一个简单的倒计时显示
/// </summary>
public partial class TimerControl : UserControl
{
    private DispatcherTimer _timer;
    private int _remainingSeconds = 180; // 默认3分钟

    public TimerControl()
    {
        InitializeComponent();
        
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        
        UpdateDisplay();
        
        // 演示用：点击开始/暂停
        MouseLeftButtonUp += (s, e) =>
        {
            if (_timer.IsEnabled)
                _timer.Stop();
            else
                _timer.Start();
            e.Handled = true;
        };
        
        // 右键重置
        MouseRightButtonUp += (s, e) =>
        {
            _timer.Stop();
            _remainingSeconds = 180;
            UpdateDisplay();
            e.Handled = true;
        };
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_remainingSeconds > 0)
        {
            _remainingSeconds--;
            UpdateDisplay();
        }
        else
        {
            _timer.Stop();
        }
    }

    private void UpdateDisplay()
    {
        var minutes = _remainingSeconds / 60;
        var seconds = _remainingSeconds % 60;
        TimerText.Text = $"{minutes:D2}:{seconds:D2}";
    }
}
