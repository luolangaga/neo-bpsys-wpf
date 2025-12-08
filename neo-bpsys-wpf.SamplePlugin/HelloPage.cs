using System.Windows.Controls;

namespace neo_bpsys_wpf.SamplePlugin;

public class HelloPage : Page
{
    public HelloPage()
    {
        Content = new TextBlock
        {
            Text = "Hello 插件页面",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center
        };
    }
}
