using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace neo_bpsys_wpf.Core.Abstractions.Services;

public record RemotePluginInfo(Guid Id, string Slug, string Name, string Version, string? Description, bool RequiresRestart, string DownloadUrl, long SizeBytes, DateTime UploadedAt);

public interface IPluginMarketplaceService
{
    Task<IReadOnlyList<RemotePluginInfo>> ListAsync(string? query = null);
    Task<(bool downloaded, bool requiresRestart, string? filePath, string? message)> DownloadAsync(RemotePluginInfo plugin, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}
