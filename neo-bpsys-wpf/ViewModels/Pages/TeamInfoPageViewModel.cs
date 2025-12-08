using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using neo_bpsys_wpf.Controls;
using System.Collections.ObjectModel;
using neo_bpsys_wpf.Core.Abstractions.Services;
using neo_bpsys_wpf.Core.Abstractions.ViewModels;
using neo_bpsys_wpf.Core.Messages;
using Player = neo_bpsys_wpf.Core.Models.Player;

namespace neo_bpsys_wpf.ViewModels.Pages;

public partial class TeamInfoPageViewModel : ViewModelBase
{
    public TeamInfoPageViewModel()
    {
        //Decorative constructor, used in conjunction with IsDesignTimeCreatable=True
    }

    public TeamInfoPageViewModel(ISharedDataService sharedDataService, IFilePickerService filePickerService,
        IMessageBoxService messageBoxService)
    {
        var sharedDataService1 = sharedDataService;
        MainTeamInfoViewModel =
            new TeamInfoViewModel(sharedDataService1.MainTeam, filePickerService, messageBoxService);
        AwayTeamInfoViewModel =
            new TeamInfoViewModel(sharedDataService1.AwayTeam, filePickerService, messageBoxService);
        OnFieldSurPlayerViewModels =
            [.. Enumerable.Range(0, 4).Select(i => new OnFieldSurPlayerViewModel(sharedDataService1, i))];
        OnFieldHunPlayerVm = new OnFieldHunPlayerViewModel(sharedDataService1);
<<<<<<< HEAD
=======

        _sharedDataService = sharedDataService1;
        _asgService = asgService;
        _messageBoxService = messageBoxService;
>>>>>>> 731ecb2 (feat(ASG): 添加玩家赛事记录功能及3D角色展示)
    }

    public TeamInfoViewModel MainTeamInfoViewModel { get; }

    public TeamInfoViewModel AwayTeamInfoViewModel { get; }

    public ObservableCollection<OnFieldSurPlayerViewModel> OnFieldSurPlayerViewModels { get; }
    public OnFieldHunPlayerViewModel OnFieldHunPlayerVm { get; }

<<<<<<< HEAD
=======
    private readonly ISharedDataService _sharedDataService;
    private readonly IASGService _asgService;
    private readonly IMessageBoxService _messageBoxService;

    

    [ObservableProperty]
    private string _eventQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<AsgEventDto> _eventResults = [];

    [ObservableProperty]
    private AsgEventDto? _selectedEvent;

    [ObservableProperty]
    private ObservableCollection<AsgMatchDto> _matchResults = [];

    [ObservableProperty]
    private AsgMatchDto? _selectedMatch;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PrevMatchesPageCommand))]
    private int _matchesPage = 1;

    [ObservableProperty]
    private int _matchesPageSize = 10;

    [ObservableProperty]
    private bool _hasMoreMatches;

    [ObservableProperty]
    private bool _isMatchResultsNotEmpty;

    

    [ObservableProperty]
    private string _newMatchResult = "Draw";

    [ObservableProperty]
    private int _newMatchScore = 0;

    [ObservableProperty]
    private int _newHunScore = 0;

    [RelayCommand]
    private async Task SearchEventsAsync()
    {
        var res = await _asgService.SearchEventsAsync(EventQuery);
        EventResults = new ObservableCollection<AsgEventDto>(res?.Items ?? Array.Empty<AsgEventDto>());
    }

    [RelayCommand]
    private async Task LoadMatchesAsync()
    {
        if (SelectedEvent == null) return;
        if (!Guid.TryParse(SelectedEvent.Id, out var eventId)) return;
        var list = await _asgService.GetMatchesByEventAsync(eventId, MatchesPage, MatchesPageSize);
        MatchResults = new ObservableCollection<AsgMatchDto>(list ?? Array.Empty<AsgMatchDto>());
        HasMoreMatches = (list?.Count ?? 0) >= MatchesPageSize;
        IsMatchResultsNotEmpty = (list?.Count ?? 0) > 0;
    }

    [RelayCommand]
    private async Task ImportSelectedMatchTeamsAsync()
    {
        if (SelectedMatch == null) return;
        if (!Guid.TryParse(SelectedMatch.HomeTeamId, out var homeId)) return;
        if (!Guid.TryParse(SelectedMatch.AwayTeamId, out var awayId)) return;
        var home = await _asgService.GetTeamAsync(homeId);
        var away = await _asgService.GetTeamAsync(awayId);
        if (home == null || away == null) return;
        var mainTeam = ConvertFromAsgTeam(home, _sharedDataService.MainTeam.Camp);
        var awayTeam = ConvertFromAsgTeam(away, _sharedDataService.AwayTeam.Camp);
        _sharedDataService.MainTeam.ImportTeamInfo(mainTeam);
        _sharedDataService.AwayTeam.ImportTeamInfo(awayTeam);
        MainTeamInfoViewModel.TeamName = _sharedDataService.MainTeam.Name;
        AwayTeamInfoViewModel.TeamName = _sharedDataService.AwayTeam.Name;
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
    private async Task CreateSurPlayersMatchRecordsAsync()
    {
        if (SelectedEvent == null)
        {
            return;
        }
        if (!Guid.TryParse(SelectedEvent.Id, out var eventId))
        {
            return;
        }
        var tasks = new List<Task<bool>>();
        foreach (var vm in OnFieldSurPlayerViewModels)
        {
            var member = vm.ThisPlayer.Member;
            if (!member.IsOnField) continue;
            if (member.AsgPlayerId == null) continue;
            var payload = new AsgPlayerMatchCreateRequest
            {
                PlayerId = member.AsgPlayerId.Value,
                EventId = eventId,
                Result = NewMatchResult,
                Score = vm.NewScore,
                PlayedAt = DateTime.UtcNow
            };
            tasks.Add(_asgService.CreatePlayerMatchAsync(payload));
        }
        var results = await Task.WhenAll(tasks);
        var successCount = results.Count(r => r);
        var total = tasks.Count;
        if (total > 0)
        {
            await _messageBoxService.ShowInfoAsync($"已为{successCount}/{total}名求生者创建赛事记录");
        }
    }

    [RelayCommand]
    private async Task CreateHunPlayerMatchRecordAsync()
    {
        if (SelectedEvent == null)
        {
            return;
        }
        if (!Guid.TryParse(SelectedEvent.Id, out var eventId))
        {
            return;
        }
        var member = OnFieldHunPlayerVm.ThisPlayer.Member;
        if (!member.IsOnField) return;
        if (member.AsgPlayerId == null) return;
        var payload = new AsgPlayerMatchCreateRequest
        {
            PlayerId = member.AsgPlayerId.Value,
            EventId = eventId,
            Result = GetOppositeResult(NewMatchResult),
            Score = NewHunScore,
            PlayedAt = DateTime.UtcNow
        };
        var ok = await _asgService.CreatePlayerMatchAsync(payload);
        if (ok)
        {
            await _messageBoxService.ShowInfoAsync("已为监管者创建赛事记录");
        }
        else
        {
            await _messageBoxService.ShowErrorAsync("创建监管者赛事记录失败");
        }
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
            _ = ImportSelectedMatchTeamsAsync();
        }
    }

    private static Core.Models.Team ConvertFromAsgTeam(AsgTeamDto t, Core.Enums.Camp camp)
    {
        var surList = new ObservableCollection<Core.Models.Member>(Enumerable.Range(0, 4).Select(_ => new Core.Models.Member(Core.Enums.Camp.Sur)));
        var hunList = new ObservableCollection<Core.Models.Member>(new[] { new Core.Models.Member(Core.Enums.Camp.Hun) });
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
        var team = new Core.Models.Team(t.Name ?? string.Empty, t.LogoUrl ?? string.Empty, surList, hunList);
        return team;
    }

>>>>>>> 731ecb2 (feat(ASG): 添加玩家赛事记录功能及3D角色展示)
    public partial class OnFieldSurPlayerViewModel : ViewModelBase
    {
        private readonly ISharedDataService _sharedDataService;

        public OnFieldSurPlayerViewModel(ISharedDataService sharedDataService, int index)
        {
            _sharedDataService = sharedDataService;
            Index = index;
            sharedDataService.CurrentGameChanged += (_, _) => OnPropertyChanged(nameof(ThisPlayer));
            sharedDataService.TeamSwapped += (_, _) => OnPropertyChanged(nameof(ThisPlayer));
        }

        public Player ThisPlayer => _sharedDataService.CurrentGame.SurPlayerList[Index];

        public int Index { get; }

        [ObservableProperty]
        private int _newScore;

        [RelayCommand]
        private void SwapMembersInPlayers(CharacterChangerCommandParameter parameter)
        {
            _sharedDataService.CurrentGame.SwapMembersInPlayers(parameter.Source, parameter.Target);
        }
    }

    public class OnFieldHunPlayerViewModel : ViewModelBase
    {
        private readonly ISharedDataService _sharedDataService;

        public OnFieldHunPlayerViewModel(ISharedDataService sharedDataService)
        {
            _sharedDataService = sharedDataService;
            sharedDataService.CurrentGameChanged += (_, _) => OnPropertyChanged(nameof(ThisPlayer));
            sharedDataService.TeamSwapped += (_, _) => OnPropertyChanged(nameof(ThisPlayer));
        }

        public Player ThisPlayer => _sharedDataService.CurrentGame.HunPlayer;
    }
}