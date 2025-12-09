using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using neo_bpsys_wpf.Core.Abstractions.Services;

namespace neo_bpsys_wpf.Services;

public class PluginMarketplaceService : IPluginMarketplaceService
{
    private readonly HttpClient _http;
    private readonly ISettingsHostService _settingsHostService;

    public PluginMarketplaceService(HttpClient http, ISettingsHostService settingsHostService)
    {
        _http = http;
        _settingsHostService = settingsHostService;
    }

    public async Task<IReadOnlyList<RemotePluginInfo>> ListAsync(string? query = null)
    {
        var baseUrl = (_settingsHostService.Settings.PluginMarketBaseUrl ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(baseUrl)) return Array.Empty<RemotePluginInfo>();
        if (!baseUrl.Contains("://")) baseUrl = "http://" + baseUrl;
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)) return Array.Empty<RemotePluginInfo>();
        var url = string.IsNullOrWhiteSpace(query) ? "/api/plugins" : $"/api/plugins?q={Uri.EscapeDataString(query)}";
        var uri = new Uri(baseUri, url);
        var res = await _http.GetAsync(uri);
        if (!res.IsSuccessStatusCode) return Array.Empty<RemotePluginInfo>();
        await using var s = await res.Content.ReadAsStreamAsync();
        var items = await JsonSerializer.DeserializeAsync<List<RemotePluginInfo>>(s, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<RemotePluginInfo>();
        return items;
    }

    public async Task<(bool downloaded, bool requiresRestart, string? filePath, string? message)> DownloadAsync(RemotePluginInfo plugin, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var baseUrl = (_settingsHostService.Settings.PluginMarketBaseUrl ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(baseUrl)) return (false, false, null, "未设置后端地址");
        if (!baseUrl.Contains("://")) baseUrl = "http://" + baseUrl;
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)) return (false, false, null, "后端地址无效");
        HttpResponseMessage res;
        try
        {
            var uri = new Uri(baseUri, $"/api/plugins/{plugin.Id}/download");
            res = await _http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (Exception ex)
        {
            return (false, false, null, $"网络错误: {ex.Message}");
        }
        if (!res.IsSuccessStatusCode) return (false, false, null, $"下载失败: {(int)res.StatusCode}");
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        Directory.CreateDirectory(dir);
        var fileName = GetFileName(res.Content.Headers, plugin) ?? $"{plugin.Slug}.{plugin.Version}.dll";
        var filePath = Path.Combine(dir, fileName);
        var requiresRestart = plugin.RequiresRestart;
        try
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            var total = res.Content.Headers.ContentLength ?? -1L;
            if (total <= 0) progress?.Report(-1.0);
            await using var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
            var buffer = new byte[81920];
            long read = 0;
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                await fs.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                read += bytesRead;
                if (total > 0)
                {
                    progress?.Report((double)read / total);
                }
            }
        }
        catch (Exception ex)
        {
            var alt = Path.Combine(dir, $"{plugin.Slug}.{plugin.Version}.dll");
            try
            {
                using var fs = new FileStream(alt, FileMode.Create, FileAccess.Write, FileShare.None);
                await res.Content.CopyToAsync(fs);
                filePath = alt;
                requiresRestart = true;
            }
            catch
            {
                return (false, false, null, $"写入文件失败: {ex.Message}");
            }
        }
        return (true, requiresRestart, filePath, null);
    }

    private static string? GetFileName(HttpContentHeaders headers, RemotePluginInfo plugin)
    {
        var cd = headers.ContentDisposition?.FileNameStar ?? headers.ContentDisposition?.FileName;
        if (!string.IsNullOrWhiteSpace(cd))
        {
            var name = cd.Trim('"');
            return Path.GetFileName(name);
        }
        return null;
    }
}
