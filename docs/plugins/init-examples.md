# 初始化场景与交互示例

在 `Initialize` 中进行运行期交互，例如订阅主窗体事件、显示弹窗等。

## 在主窗体 Loaded 弹出登录

参考 ASG 插件：

- 初始化入口：`plugins/ASG/AsgPlugin.cs:25`
- 订阅主窗体 Loaded：`plugins/ASG/AsgPlugin.cs:30`
- 弹窗登录与保存账号：`plugins/ASG/AsgPlugin.cs:59`、`plugins/ASG/AsgPlugin.cs:75`

关键点：

- 获取服务与设置：`context.Services.GetRequiredService<T>()`
- 避免命名冲突：使用 `using MessageBox = Wpf.Ui.Controls.MessageBox;`（`plugins/ASG/AsgPlugin.cs:9`）

## 访问设置并响应变更

- 读取设置：`neo-bpsys-wpf/Services/SettingsHostService.cs:70`
- 保存设置：`neo-bpsys-wpf/Services/SettingsHostService.cs:50`
- 设置模型：`neo-bpsys-wpf.Core/Models/Settings.cs:16`、`neo-bpsys-wpf.Core/Models/Settings.cs:18`、`neo-bpsys-wpf.Core/Models/Settings.cs:20`

## 异步与延时

在 UI 已加载后进行异步操作，可在 `Loaded` 事件中 `await Task.Delay(...)` 以等待界面就绪。

## 代码示例：修改软件内数据（屏蔽底层实现）

```csharp
using Microsoft.Extensions.DependencyInjection;
using neo_bpsys_wpf.Core.Abstractions.Extensions;
using neo_bpsys_wpf.Core.Abstractions.Services;
using neo_bpsys_wpf.Core.Enums;
using neo_bpsys_wpf.Core.Models;

public class DataDemoPlugin : IPlugin
{
    public PluginMetadata Metadata { get; } = new() { Id = "bpsys.data.demo", Name = "数据示例" };

    public void Initialize(IPluginContext context)
    {
        var data = context.Services.GetRequiredService<ISharedDataService>();

        data.CurrentGame.GameProgress = GameProgress.Game1FirstHalf;
        data.CurrentGame.PickMap(Map.红教堂, data.CurrentGame.SurTeam);

        var surA = data.MainTeam.SurMemberList[0];
        var surB = data.MainTeam.SurMemberList[1];
        var hun = data.AwayTeam.HunMemberList[0];
        surA.Name = "选手A";
        surB.Name = "选手B";
        hun.Name = "监管者X";
        data.MainTeam.MemberOnField(surA);
        data.MainTeam.MemberOnField(surB);
        data.AwayTeam.MemberOnField(hun);

        data.CurrentGame.SurPlayerList[0].Character = data.SurCharaList["机械师"];
        data.CurrentGame.SurPlayerList[1].Character = data.SurCharaList["前锋"];
        data.CurrentGame.HunPlayer.Character = data.HunCharaList["小丑"];

        data.CurrentGame.SurPlayerList[0].Talent.FlywheelEffect = true;
        data.CurrentGame.SurPlayerList[1].Talent.BorrowedTime = true;
        data.CurrentGame.HunPlayer.Trait = new Trait(TraitType.传送);

        data.CurrentGame.SurPlayerList[0].Data.MachineDecoded = "110%";
        data.CurrentGame.SurPlayerList[0].Data.KiteTime = "120s";
        data.CurrentGame.HunPlayer.Data.HitTimes = "6";

        data.CurrentGame.SurTeam.Score.MinorPoints += 3;
        data.CurrentGame.HunTeam.Score.Win += 1;

        data.SetBanCount(BanListName.CanCurrentSurBanned, 2);
        data.SetBanCount(BanListName.CanCurrentHunBanned, 1);
        data.CurrentGame.CurrentSurBannedList[0] = data.SurCharaList["机械师"];
        data.CurrentGame.CurrentHunBannedList[0] = data.HunCharaList["小丑"];

        data.TimerStart(60);
        data.IsBo3Mode = true;
        data.IsTraitVisible = false;

        data.CurrentGame.Swap();
    }

    public IEnumerable<PluginNavigationItem> GetMenuItems() => Array.Empty<PluginNavigationItem>();
    public IEnumerable<PluginNavigationItem> GetFooterItems() => Array.Empty<PluginNavigationItem>();
    public void ConfigureServices(IServiceCollection services) {}
}
```

## 可修改数据总览与对应 API

- 对局进度：`Game.GameProgress`（`neo-bpsys-wpf.Core/Models/Game.cs:68`）
- 地图选择/重置：`Game.PickMap`、`Game.ResetMapBp`（`neo-bpsys-wpf.Core/Models/Game.cs:203`、`neo-bpsys-wpf.Core/Models/Game.cs:231`）
- 上场选手：`Team.MemberOnField`、`Team.MemberOffField`（`neo-bpsys-wpf.Core/Models/Team.cs:240`、`neo-bpsys-wpf.Core/Models/Team.cs:268`）
- 当前上场选手对象：`Game.SurPlayerList[i]`、`Game.HunPlayer`（`neo-bpsys-wpf.Core/Models/Game.cs:131`、`neo-bpsys-wpf.Core/Models/Game.cs:133`）
- 角色选择：`Player.Character`（`neo-bpsys-wpf.Core/Models/Player.cs:32`），角色字典：`ISharedDataService.SurCharaList/HunCharaList`
- 天赋与特质：`Player.Talent.*`、`Player.Trait = new Trait(...)`（`neo-bpsys-wpf.Core/Models/Talent.cs`、`neo-bpsys-wpf.Core/Models/Trait.cs`）
- 赛后数据：`Player.Data.*`（`neo-bpsys-wpf.Core/Models/PlayerData.cs`）
- 比分：`Team.Score.Win/Tie/MinorPoints`（`neo-bpsys-wpf.Core/Models/Score.cs`）
- 当局 Ban 位：`Game.CurrentSurBannedList/CurrentHunBannedList`（`neo-bpsys-wpf.Core/Models/Game.cs:74`、`neo-bpsys-wpf.Core/Models/Game.cs:76`），数量控制：`ISharedDataService.SetBanCount`（`neo-bpsys-wpf/Services/SharedDataService.cs:208`）
- 倒计时：`ISharedDataService.TimerStart/TimerStop`、`RemainingSeconds`（`neo-bpsys-wpf/Services/SharedDataService.cs:283`、`neo-bpsys-wpf/Services/SharedDataService.cs:292`）
- 模式与显示：`IsBo3Mode`、`IsTraitVisible`、`IsMapV2Breathing`、`IsMapV2CampVisible`（`neo-bpsys-wpf/Services/SharedDataService.cs:325`、`neo-bpsys-wpf/Services/SharedDataService.cs:307`、`neo-bpsys-wpf/Services/SharedDataService.cs:365`、`neo-bpsys-wpf/Services/SharedDataService.cs:385`）
- 换边：`Game.Swap()`（`neo-bpsys-wpf.Core/Models/Game.cs:250`）
