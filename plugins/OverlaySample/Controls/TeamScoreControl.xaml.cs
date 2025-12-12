using System.Windows.Controls;
using System.Windows.Input;

namespace Bpsys.Plugin.OverlaySample.Controls;

/// <summary>
/// 队伍得分控件 - 展示两队比分
/// </summary>
public partial class TeamScoreControl : UserControl
{
    private int _team1Score = 0;
    private int _team2Score = 0;

    public TeamScoreControl()
    {
        InitializeComponent();
        
        // 演示用：点击左侧增加主队分数，点击右侧增加客队分数
        MouseLeftButtonUp += (s, e) =>
        {
            var pos = e.GetPosition(this);
            if (pos.X < ActualWidth / 2)
            {
                _team1Score++;
                Team1Score.Text = _team1Score.ToString();
            }
            else
            {
                _team2Score++;
                Team2Score.Text = _team2Score.ToString();
            }
            e.Handled = true;
        };
        
        // 右键重置
        MouseRightButtonUp += (s, e) =>
        {
            _team1Score = 0;
            _team2Score = 0;
            Team1Score.Text = "0";
            Team2Score.Text = "0";
            e.Handled = true;
        };
    }
}
