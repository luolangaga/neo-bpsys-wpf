#pragma warning disable CA1416 // Windows-only plugin, suppress platform compatibility analyzer noise
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System;
using neo_bpsys_wpf.Core.Abstractions.Services;
using neo_bpsys_wpf.Core.Abstractions.ViewModels;
using neo_bpsys_wpf.Core.Enums;
using neo_bpsys_wpf.Core.Models;

namespace Bpsys.Plugin.ASG.ViewModels.Pages;

public partial class AsgHelperPageViewModel : ViewModelBase
{
    private readonly IASGService _asgService;
    private readonly ISettingsHostService _settingsHostService;
    private readonly ISharedDataService _sharedDataService;
    private readonly IMessageBoxService _messageBoxService;

    public AsgHelperPageViewModel(IASGService asgService, ISettingsHostService settingsHostService,
        ISharedDataService sharedDataService, IMessageBoxService messageBoxService)
    {
        _asgService = asgService;
        _settingsHostService = settingsHostService;
        _sharedDataService = sharedDataService;
        _messageBoxService = messageBoxService;
        AsgEmail = _settingsHostService.Settings.AsgEmail ?? string.Empty;
        AsgPassword = _settingsHostService.Settings.AsgPassword ?? string.Empty;
    }

    [ObservableProperty] private string _asgEmail = string.Empty;
    [ObservableProperty] private string _asgPassword = string.Empty;
    [ObservableProperty] private bool _isLoggingIn;

    [ObservableProperty] private string _eventQuery = string.Empty;
    [ObservableProperty] private ObservableCollection<AsgEventDto> _eventResults = [];
    [ObservableProperty] private AsgEventDto? _selectedEvent;
    [ObservableProperty] private ObservableCollection<AsgMatchDto> _matchResults = [];
    [ObservableProperty] private AsgMatchDto? _selectedMatch;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PrevMatchesPageCommand))]
    private int _matchesPage = 1;

    [ObservableProperty] private int _matchesPageSize = 10;
    [ObservableProperty] private bool _hasMoreMatches;
    [ObservableProperty] private bool _isMatchResultsNotEmpty;

    [RelayCommand(CanExecute = nameof(CanLoginExecute))]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(AsgEmail) || string.IsNullOrWhiteSpace(AsgPassword))
        {
            await _messageBoxService.ShowErrorAsync("请输入邮箱和密码");
            return;
        }

        IsLoggingIn = true;
        try
        {
            var ok = await _asgService.LoginAsync(AsgEmail, AsgPassword);
            if (ok)
            {
                _settingsHostService.Settings.AsgEmail = AsgEmail;
                _settingsHostService.Settings.AsgPassword = AsgPassword;
                _settingsHostService.SaveConfig();
                await _messageBoxService.ShowInfoAsync("登录成功");
            }
            else
            {
                await _messageBoxService.ShowErrorAsync("登录失败，请检查账号密码");
            }
        }
        catch (Exception ex)
        {
            await _messageBoxService.ShowErrorAsync($"登录异常：{ex.Message}");
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    private bool CanLoginExecute() => !IsLoggingIn;

    [RelayCommand]
    private async Task SearchEventsAsync()
    {
        try
        {
            var res = await _asgService.SearchEventsAsync(EventQuery);
            EventResults = new ObservableCollection<AsgEventDto>(res?.Items ?? Array.Empty<AsgEventDto>());
        }
        catch (Exception ex)
        {
            await _messageBoxService.ShowErrorAsync($"搜索失败：{ex.Message}");
        }
    }

    [RelayCommand]
    private async Task LoadMatchesAsync()
    {
        if (SelectedEvent == null) return;
        if (!Guid.TryParse(SelectedEvent.Id, out var eventId)) return;
        try
        {
            var list = await _asgService.GetMatchesByEventAsync(eventId, MatchesPage, MatchesPageSize);
            MatchResults = new ObservableCollection<AsgMatchDto>(list ?? Array.Empty<AsgMatchDto>());
            HasMoreMatches = (list?.Count ?? 0) >= MatchesPageSize;
            IsMatchResultsNotEmpty = (list?.Count ?? 0) > 0;
        }
        catch (Exception ex)
        {
            await _messageBoxService.ShowErrorAsync($"加载赛程失败：{ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanPrevMatchesPageExecute))]
    private async Task PrevMatchesPage()
    {
        if (MatchesPage <= 1) return;
        MatchesPage -= 1;
        await LoadMatchesAsync();
    }

    private bool CanPrevMatchesPageExecute() => MatchesPage > 1;

    [RelayCommand]
    private async Task NextMatchesPage()
    {
        MatchesPage += 1;
        await LoadMatchesAsync();
        if (!HasMoreMatches)
        {
            MatchesPage -= 1;
        }
    }

    [RelayCommand]
    private async Task RefreshMatches()
    {
        MatchesPage = Math.Max(1, MatchesPage);
        await LoadMatchesAsync();
    }

    [RelayCommand]
    private async Task ImportSelectedMatchTeamsAsync()
    {
        if (SelectedMatch == null) return;
        if (!Guid.TryParse(SelectedMatch.HomeTeamId, out var homeId)) return;
        if (!Guid.TryParse(SelectedMatch.AwayTeamId, out var awayId)) return;
        try
        {
            var home = await _asgService.GetTeamAsync(homeId);
            var away = await _asgService.GetTeamAsync(awayId);
            if (home == null || away == null) return;
            var mainTeam = ConvertFromAsgTeam(home, _sharedDataService.MainTeam.Camp);
            var awayTeam = ConvertFromAsgTeam(away, _sharedDataService.AwayTeam.Camp);
            _sharedDataService.MainTeam.ImportTeamInfo(mainTeam);
            _sharedDataService.AwayTeam.ImportTeamInfo(awayTeam);
            _sharedDataService.SelectedMatchId = Guid.TryParse(SelectedMatch.Id, out var mid) ? mid : null;
            await _messageBoxService.ShowInfoAsync("已导入队伍信息");
        }
        catch (Exception ex)
        {
            await _messageBoxService.ShowErrorAsync($"导入失败：{ex.Message}");
        }
    }

    partial void OnSelectedEventChanged(AsgEventDto? value)
    {
        MatchesPage = 1;
        MatchResults.Clear();
        IsMatchResultsNotEmpty = false;
    }

    partial void OnSelectedMatchChanged(AsgMatchDto? value)
    {
        if (value == null)
        {
            _sharedDataService.SelectedMatchId = null;
            return;
        }

        if (Guid.TryParse(value.Id, out var matchId))
        {
            _sharedDataService.SelectedMatchId = matchId;
        }
    }

    private static Team ConvertFromAsgTeam(AsgTeamDto t, Camp camp)
    {
        var surList = new ObservableCollection<Member>(Enumerable.Range(0, 4).Select(_ => new Member(Camp.Sur)));
        var hunList = new ObservableCollection<Member>(new[] { new Member(Camp.Hun) });
        var players = t.Players ?? Array.Empty<AsgPlayerDto>();
        for (var i = 0; i < Math.Min(4, players.Length); i++)
        {
            var p = players[i];
            surList[i].Name = p.Name ?? string.Empty;
            if (Guid.TryParse(p.Id, out var gid)) surList[i].AsgPlayerId = gid;
        }
        if (players.Length > 0)
        {
            var p = players[0];
            hunList[0].Name = p.Name ?? string.Empty;
            if (Guid.TryParse(p.Id, out var gid)) hunList[0].AsgPlayerId = gid;
        }
        var team = new Team(t.Name ?? string.Empty, t.LogoUrl ?? string.Empty, surList, hunList);
        team.Camp = camp;
        return team;
    }

    private static string GetOppositeResult(string result)
    {
        return result switch
        {
            "Win" => "Loss",
            "Loss" => "Win",
            _ => "Draw"
        };
    }
}
#pragma warning restore CA1416
