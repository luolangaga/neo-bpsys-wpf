namespace PluginMarket.Api.Models;

public record UploadPluginRequest(string Slug, string Name, string Version, string? Description, bool RequiresRestart);
public record PluginDto(Guid Id, string Slug, string Name, string Version, string? Description, bool RequiresRestart, string Status, string DownloadUrl, long SizeBytes, DateTime UploadedAt);
public record ReviewRequest(string? Reason);
