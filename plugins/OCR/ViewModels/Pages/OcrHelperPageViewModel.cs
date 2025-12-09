using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using neo_bpsys_wpf.Core;
using neo_bpsys_wpf.Core.Abstractions.Services;
using neo_bpsys_wpf.Core.Abstractions.ViewModels;
using neo_bpsys_wpf.Core.Models;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using System.Windows;
using System.Windows.Threading;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bpsys.Plugin.OCR.ViewModels.Pages;

public partial class OcrHelperPageViewModel : ViewModelBase, IDisposable
{
    public OcrHelperPageViewModel() { }

    private readonly ISharedDataService _sharedDataService;
    private readonly ISettingsHostService _settingsHostService;
    private readonly IOcrModelService _ocrModelService;

    private DispatcherTimer? _ocrTimer;
    private PaddleOcrAll? _ocrAll;
    private CancellationTokenSource? _ocrDownloadCts;
    private bool _isOcrRunning;
    private bool _preferMkldnn = true;
    private int _ocrRunCounter;
    private const int OcrRecycleThreshold = 60;

    [ObservableProperty] private bool _isOcrRecognizing;
    [ObservableProperty] private bool _isOcrModelDownloading;
    [ObservableProperty] private bool _isPickRowModeEnabled;
    [ObservableProperty] private bool _isBanRowModeEnabled;

    public OcrHelperPageViewModel(ISharedDataService sharedDataService, ISettingsHostService settingsHostService, IOcrModelService ocrModelService)
    {
        _sharedDataService = sharedDataService;
        _settingsHostService = settingsHostService;
        _ocrModelService = ocrModelService;
        _isPickRowModeEnabled = _settingsHostService.Settings.BpWindowSettings.PickOcrRowMode;
        _isBanRowModeEnabled = _settingsHostService.Settings.BpWindowSettings.BanOcrRowMode;
    }

    partial void OnIsOcrRecognizingChanged(bool value)
    {
        if (value) StartOcrTimer(); else StopOcrTimer();
    }

    [RelayCommand]
    private void SelectPickOcrRowRegions()
    {
        var labels = new[] { "框选求生者一排", "框选监管者" };
        var win = new neo_bpsys_wpf.Views.Windows.RegionSelectorWindow(labels)
        {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        win.ShowDialog();
        var regions = win.Regions;
        if (regions.Count == 2)
        {
            var bp = _settingsHostService.Settings.BpWindowSettings;
            bp.PickOcrRowRegions = [.. regions];
            bp.PickOcrRowMode = true;
            IsPickRowModeEnabled = true;
            _settingsHostService.SaveConfig();
        }
    }

    [RelayCommand]
    private void SelectBanSurOcrRowRegion()
    {
        var labels = new[] { "框选禁用求生一排" };
        var win = new neo_bpsys_wpf.Views.Windows.RegionSelectorWindow(labels)
        {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        win.ShowDialog();
        var regions = win.Regions;
        if (regions.Count >= 1)
        {
            var bp = _settingsHostService.Settings.BpWindowSettings;
            bp.BanSurOcrRowRegion = regions[0];
            bp.BanOcrRowMode = true;
            IsBanRowModeEnabled = true;
            _settingsHostService.SaveConfig();
        }
    }

    [RelayCommand]
    private void SelectBanHunOcrRowRegion()
    {
        var labels = new[] { "框选禁用监管一排" };
        var win = new neo_bpsys_wpf.Views.Windows.RegionSelectorWindow(labels)
        {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        win.ShowDialog();
        var regions = win.Regions;
        if (regions.Count >= 1)
        {
            var bp = _settingsHostService.Settings.BpWindowSettings;
            bp.BanHunOcrRowRegion = regions[0];
            bp.BanOcrRowMode = true;
            IsBanRowModeEnabled = true;
            _settingsHostService.SaveConfig();
        }
    }

    partial void OnIsPickRowModeEnabledChanged(bool value)
    {
        _settingsHostService.Settings.BpWindowSettings.PickOcrRowMode = value;
        _settingsHostService.SaveConfig();
    }

    partial void OnIsBanRowModeEnabledChanged(bool value)
    {
        _settingsHostService.Settings.BpWindowSettings.BanOcrRowMode = value;
        _settingsHostService.SaveConfig();
    }

    private void StartOcrTimer()
    {
        StopOcrTimer();
        _ocrTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(4) };
        _ocrTimer.Tick += async (_, _) =>
        {
            try
            {
                if (_ocrAll == null)
                {
                    IsOcrModelDownloading = true;
                    _ocrDownloadCts = new CancellationTokenSource();
                    try
                    {
                        var spec = _settingsHostService.Settings.OcrSettings.ModelSpec;
                        var mirror = _settingsHostService.Settings.OcrSettings.Mirror;
                        var model = await _ocrModelService.EnsureAsync(spec, mirror, _ocrDownloadCts.Token);
                        _ocrAll = new PaddleOcrAll(model, _preferMkldnn ? PaddleDevice.Mkldnn() : PaddleDevice.Blas())
                        {
                            AllowRotateDetection = false,
                            Enable180Classification = false
                        };
                    }
                    finally
                    {
                        Application.Current.Dispatcher.Invoke(() => { IsOcrModelDownloading = false; });
                        _ocrDownloadCts?.Dispose();
                        _ocrDownloadCts = null;
                    }
                }

                var bp = _settingsHostService.Settings.BpWindowSettings;

                if (_isOcrRunning) return;
                _isOcrRunning = true;
                if (IsPickRowModeEnabled && bp.PickOcrRowRegions is { Count: 2 })
                {
                    var resSur = await Task.Run(() => RecognizeResult(bp.PickOcrRowRegions[0]));
                    var tokensSur = ExtractRowTokens(resSur, 4);
                    for (var i = 0; i < tokensSur.Count; i++)
                    {
                        var ch = FindBestCharacterFuzzy(tokensSur[i], _sharedDataService.SurCharaList.Values);
                        if (ch != null && i < _sharedDataService.CurrentGame.SurPlayerList.Count)
                            _sharedDataService.CurrentGame.SurPlayerList[i].Character = ch;
                    }

                    var resHun = await Task.Run(() => RecognizeResult(bp.PickOcrRowRegions[1]));
                    var tokensHun = ExtractRowTokens(resHun, 4);
                    if (tokensHun.Count > 0)
                    {
                        var chHun = FindBestCharacterFuzzy(tokensHun[0], _sharedDataService.HunCharaList.Values);
                        if (chHun != null)
                            _sharedDataService.CurrentGame.HunPlayer.Character = chHun;
                    }
                }

                if (IsBanRowModeEnabled)
                {
                    if (bp.BanSurOcrRowRegion.Width > 0 && bp.BanSurOcrRowRegion.Height > 0)
                    {
                        var resBanSur = await Task.Run(() => RecognizeResult(bp.BanSurOcrRowRegion));
                        var tokensBanSur = ExtractRowTokens(resBanSur, 12);
                        var idx = 0;
                        foreach (var tk in tokensBanSur)
                        {
                            if (idx >= neo_bpsys_wpf.Core.AppConstants.CurrentBanSurCount) break;
                            var ch = FindBestCharacterFuzzy(tk, _sharedDataService.SurCharaList.Values);
                            if (ch != null)
                            {
                                _sharedDataService.CurrentGame.CurrentSurBannedList[idx] = ch;
                                idx++;
                            }
                        }
                    }
                    if (bp.BanHunOcrRowRegion.Width > 0 && bp.BanHunOcrRowRegion.Height > 0)
                    {
                        var resBanHun = await Task.Run(() => RecognizeResult(bp.BanHunOcrRowRegion));
                        var tokensBanHun = ExtractRowTokens(resBanHun, 12);
                        var idx = 0;
                        foreach (var tk in tokensBanHun)
                        {
                            if (idx >= neo_bpsys_wpf.Core.AppConstants.CurrentBanHunCount) break;
                            var ch = FindBestCharacterFuzzy(tk, _sharedDataService.HunCharaList.Values);
                            if (ch != null)
                            {
                                _sharedDataService.CurrentGame.CurrentHunBannedList[idx] = ch;
                                idx++;
                            }
                        }
                    }
                }
                if (_ocrRunCounter >= OcrRecycleThreshold)
                {
                    _ocrAll?.Dispose();
                    _ocrAll = null;
                    _ocrRunCounter = 0;
                    CompactGc();
                }
                _isOcrRunning = false;
            }
            catch
            {
                _isOcrRunning = false;
            }
        };
        _ocrTimer.Start();
    }

    private static void CompactGc()
    {
        System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false, true);
    }

    private void StopOcrTimer()
    {
        _ocrTimer?.Stop();
        _ocrTimer = null;
        _ocrDownloadCts?.Cancel();
        _ocrDownloadCts?.Dispose();
        _ocrDownloadCts = null;
    }

    private PaddleOcrResult? RecognizeResult(Int32Rect rect)
    {
        if (_ocrAll == null) return null;
        using var bmp = Capture(rect);
        using var mat = bmp.ToMat();
        var use = EnsureMatSize(mat);
        try
        {
            var r = _ocrAll.Run(use);
            _ocrRunCounter++;
            return r;
        }
        catch
        {
            _preferMkldnn = false;
            try { _ocrAll?.Dispose(); } catch { }
            _ocrAll = null;
            return null;
        }
        finally
        {
            if (!ReferenceEquals(use, mat)) use.Dispose();
        }
    }

    private static System.Drawing.Bitmap Capture(Int32Rect rect)
    {
        using var bmp = new System.Drawing.Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        using (var g = System.Drawing.Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(rect.X, rect.Y, 0, 0, new System.Drawing.Size(rect.Width, rect.Height));
        }
        return (System.Drawing.Bitmap)bmp.Clone();
    }

    private static Mat EnsureMatSize(Mat mat)
    {
        const int minSize = 128;
        var w = mat.Width;
        var h = mat.Height;
        if (w >= minSize && h >= minSize) return mat;
        var scale = Math.Max(1.0, Math.Max((double)minSize / Math.Max(1, w), (double)minSize / Math.Max(1, h)));
        var newW = (int)Math.Round(w * scale);
        var newH = (int)Math.Round(h * scale);
        var dst = new Mat();
        Cv2.Resize(mat, dst, new OpenCvSharp.Size(newW, newH), 0, 0, InterpolationFlags.Linear);
        return dst;
    }

    private static List<string> ExtractRowTokens(PaddleOcrResult? result, int maxCount)
    {
        var tokens = new List<string>();
        if (result == null) return tokens;
        foreach (var item in result.Regions)
        {
            var norm = Normalize(item.Text);
            if (!string.IsNullOrEmpty(norm)) tokens.Add(norm);
            if (tokens.Count >= maxCount) break;
        }
        return tokens;
    }

    private static string Normalize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        var t = s.Replace("\r", " ").Replace("\n", " ");
        t = new string(t.Where(ch => !char.IsWhiteSpace(ch) && !char.IsPunctuation(ch)).ToArray());
        return t.ToLowerInvariant();
    }

    private static Character? FindBestCharacterFuzzy(string token, IEnumerable<Character> candidates)
    {
        if (string.IsNullOrEmpty(token)) return null;
        var best = candidates.Select(ch => (Ch: ch, Score: ScoreSimilarity(token, Normalize(ch.Name))))
            .OrderByDescending(x => x.Score).FirstOrDefault();
        return best.Score < 0.45 ? null : best.Ch;
    }

    private static double ScoreSimilarity(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;
        var min = Math.Min(a.Length, b.Length);
        var common = 0;
        for (var i = 0; i < min; i++) if (a[i] == b[i]) common++;
        return (double)common / Math.Max(1, Math.Max(a.Length, b.Length));
    }

    public void Dispose()
    {
        try { _ocrAll?.Dispose(); } catch { }
        _ocrAll = null;
        _ocrTimer?.Stop();
        _ocrTimer = null;
        _ocrDownloadCts?.Cancel();
        _ocrDownloadCts?.Dispose();
        _ocrDownloadCts = null;
    }
}
