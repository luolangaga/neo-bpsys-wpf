using System;
using System.Text.Json.Serialization;

namespace neo_bpsys_wpf.Core.Models;

public class AsgEventDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("logoUrl")] public string? LogoUrl { get; set; }
}

public class AsgEventDtoPagedResult
{
    [JsonPropertyName("items")] public AsgEventDto[] Items { get; set; } = Array.Empty<AsgEventDto>();
    [JsonPropertyName("totalCount")] public int TotalCount { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
}

public class AsgMatchDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("eventId")] public string? EventId { get; set; }
    [JsonPropertyName("homeTeamId")] public string HomeTeamId { get; set; } = string.Empty;
    [JsonPropertyName("homeTeamName")] public string HomeTeamName { get; set; } = string.Empty;
    [JsonPropertyName("awayTeamId")] public string AwayTeamId { get; set; } = string.Empty;
    [JsonPropertyName("awayTeamName")] public string AwayTeamName { get; set; } = string.Empty;
}

public class AsgTeamDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("logoUrl")] public string? LogoUrl { get; set; }
    [JsonPropertyName("players")] public AsgPlayerDto[] Players { get; set; } = Array.Empty<AsgPlayerDto>();
}

public class AsgPlayerDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

public class AsgMatchScoresUpdateDto
{
    [JsonPropertyName("bestOf")] public int BestOf { get; set; }
    [JsonPropertyName("games")] public AsgGameScoreDto[] Games { get; set; } = Array.Empty<AsgGameScoreDto>();
}

public class AsgGameScoreDto
{
    [JsonPropertyName("home")] public int Home { get; set; }
    [JsonPropertyName("away")] public int Away { get; set; }
}

public class AsgPlayerMatchCreateRequest
{
    [JsonPropertyName("playerId")] public Guid PlayerId { get; set; }
    [JsonPropertyName("eventId")] public Guid EventId { get; set; }
    [JsonPropertyName("gameRoleId")] public Guid? GameRoleId { get; set; }
    [JsonPropertyName("result")] public string Result { get; set; } = string.Empty;
    [JsonPropertyName("score")] public int Score { get; set; }
    [JsonPropertyName("playedAt")] public DateTime? PlayedAt { get; set; }
}

public class AsgPlayerStatsDto
{
    [JsonPropertyName("totalMatches")] public int TotalMatches { get; set; }
    [JsonPropertyName("winRate")] public double WinRate { get; set; }
    [JsonPropertyName("lossRate")] public double LossRate { get; set; }
    [JsonPropertyName("drawRate")] public double DrawRate { get; set; }
    [JsonPropertyName("recentMatches")] public AsgRecentMatchDto[] RecentMatches { get; set; } = Array.Empty<AsgRecentMatchDto>();
}

public class AsgRecentMatchDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("playerId")] public Guid PlayerId { get; set; }
    [JsonPropertyName("eventId")] public Guid EventId { get; set; }
    [JsonPropertyName("gameRoleId")] public Guid? GameRoleId { get; set; }
    [JsonPropertyName("result")] public string Result { get; set; } = string.Empty;
    [JsonPropertyName("score")] public int Score { get; set; }
    [JsonPropertyName("playedAt")] public DateTime PlayedAt { get; set; }
    [JsonPropertyName("eventName")] public string? EventName { get; set; }
    [JsonPropertyName("gameRoleName")] public string? GameRoleName { get; set; }
}
