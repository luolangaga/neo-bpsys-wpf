using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Assimp;
using HelixToolkit.SharpDX.Model;

namespace neo_bpsys_wpf.Controls;

public partial class CharacterVisual : UserControl
{
    public CharacterVisual()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register(
        nameof(Player),
        typeof(neo_bpsys_wpf.Core.Models.Player),
        typeof(CharacterVisual),
        new PropertyMetadata(null)
    );

    public neo_bpsys_wpf.Core.Models.Player? Player
    {
        get => (neo_bpsys_wpf.Core.Models.Player?)GetValue(PlayerProperty);
        set => SetValue(PlayerProperty, value);
    }

    public static readonly DependencyProperty ConfigPathProperty = DependencyProperty.Register(
        nameof(ConfigPath),
        typeof(string),
        typeof(CharacterVisual),
        new PropertyMetadata(null, OnConfigPathChanged)
    );

    public string? ConfigPath
    {
        get => (string?)GetValue(ConfigPathProperty);
        set => SetValue(ConfigPathProperty, value);
    }

    private static void OnConfigPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CharacterVisual control)
        {
            control.LoadConfig();
        }
    }

    public static readonly DependencyProperty IsConfigAvailableProperty = DependencyProperty.Register(
        nameof(IsConfigAvailable), typeof(bool), typeof(CharacterVisual), new PropertyMetadata(false)
    );

    public bool IsConfigAvailable
    {
        get => (bool)GetValue(IsConfigAvailableProperty);
        set => SetValue(IsConfigAvailableProperty, value);
    }

    public static readonly DependencyProperty SceneContentProperty = DependencyProperty.Register(
        nameof(SceneContent), typeof(Element3D), typeof(CharacterVisual), new PropertyMetadata(null)
    );

    public Element3D? SceneContent
    {
        get => (Element3D?)GetValue(SceneContentProperty);
        set => SetValue(SceneContentProperty, value);
    }

    private SceneNodeGroupModel3D? _sceneGroup;
    private AxisAngleRotation3D? _yawRotation;
    private AxisAngleRotation3D? _pitchRotation;
    private AxisAngleRotation3D? _rollRotation;

    private sealed class Character3DConfig
    {
        public string ModelPath { get; set; } = string.Empty;
        public string? AnimationPath { get; set; }
        public string? AnimationName { get; set; }
        public int? AnimationIndex { get; set; }
        public double? Scale { get; set; }
        public double? RotateYaw { get; set; }
        public double? RotatePitch { get; set; }
        public double? RotateRoll { get; set; }
    }

    private sealed class CharacterAnimationConfig
    {
        public double? YawSpeed { get; set; }
        public double? PitchAmplitude { get; set; }
        public double? PitchPeriodSec { get; set; }
        public double? RollAmplitude { get; set; }
        public double? RollPeriodSec { get; set; }
    }

    private void StopAnimations()
    {
        _yawRotation?.BeginAnimation(AxisAngleRotation3D.AngleProperty, null);
        _pitchRotation?.BeginAnimation(AxisAngleRotation3D.AngleProperty, null);
        _rollRotation?.BeginAnimation(AxisAngleRotation3D.AngleProperty, null);
    }

    private void LoadConfig()
    {
        try
        {
            var configPath = Environment.ExpandEnvironmentVariables(ConfigPath ?? string.Empty);
            if (string.IsNullOrWhiteSpace(configPath) || !File.Exists(configPath))
            {
                IsConfigAvailable = false;
                SceneContent = null;
                StopAnimations();
                return;
            }

            var json = File.ReadAllText(configPath);
            var cfg = JsonSerializer.Deserialize<Character3DConfig>(json);
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.ModelPath))
            {
                IsConfigAvailable = false;
                SceneContent = null;
                StopAnimations();
                return;
            }

            var baseDir = Path.GetDirectoryName(configPath) ?? AppDomain.CurrentDomain.BaseDirectory;
            var modelPath = Environment.ExpandEnvironmentVariables(cfg.ModelPath);
            if (!Path.IsPathRooted(modelPath))
                modelPath = Path.Combine(baseDir, modelPath);

            if (!File.Exists(modelPath))
            {
                IsConfigAvailable = false;
                SceneContent = null;
                StopAnimations();
                return;
            }

            var importer = new Importer();
            var scene = importer.Load(modelPath);

            if (scene != null && scene.Root != null)
            {
                _sceneGroup = new SceneNodeGroupModel3D();
                var tr = new Transform3DGroup();
                if (cfg.Scale.HasValue)
                {
                    var s = cfg.Scale.Value;
                    tr.Children.Add(new ScaleTransform3D(s, s, s));
                }
                _yawRotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), cfg.RotateYaw ?? 0);
                _pitchRotation = new AxisAngleRotation3D(new Vector3D(1, 0, 0), cfg.RotatePitch ?? 0);
                _rollRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), cfg.RotateRoll ?? 0);
                tr.Children.Add(new RotateTransform3D(_yawRotation));
                tr.Children.Add(new RotateTransform3D(_pitchRotation));
                tr.Children.Add(new RotateTransform3D(_rollRotation));
                if (tr.Children.Count > 0)
                    _sceneGroup.Transform = tr;
                _sceneGroup.AddNode(scene.Root);
                SceneContent = _sceneGroup;
                IsConfigAvailable = true;
                StopAnimations();
                StartRotationAnimation(cfg, baseDir);
            }
            else
            {
                IsConfigAvailable = false;
                SceneContent = null;
                StopAnimations();
            }
        }
        catch
        {
            IsConfigAvailable = false;
            SceneContent = null;
            StopAnimations();
        }
    }

    private void StartRotationAnimation(Character3DConfig cfg, string baseDir)
    {
        CharacterAnimationConfig? animCfg = null;
        if (!string.IsNullOrWhiteSpace(cfg.AnimationPath))
        {
            var ap = Environment.ExpandEnvironmentVariables(cfg.AnimationPath);
            if (!Path.IsPathRooted(ap))
                ap = Path.Combine(baseDir, ap);
            if (File.Exists(ap))
            {
                try
                {
                    var animJson = File.ReadAllText(ap);
                    animCfg = JsonSerializer.Deserialize<CharacterAnimationConfig>(animJson);
                }
                catch
                {
                }
            }
        }
        var yawSpeed = animCfg?.YawSpeed ?? 20.0;
        if (_yawRotation != null)
        {
            var d = new Duration(TimeSpan.FromSeconds(360.0 / Math.Max(1.0, yawSpeed)));
            var a = new DoubleAnimation
            {
                From = _yawRotation.Angle,
                To = _yawRotation.Angle + 360.0,
                Duration = d,
                RepeatBehavior = RepeatBehavior.Forever
            };
            _yawRotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, a);
        }

        var pitchAmp = animCfg?.PitchAmplitude;
        var pitchPeriod = animCfg?.PitchPeriodSec;
        if (_pitchRotation != null && pitchAmp.HasValue && pitchPeriod.HasValue && pitchPeriod.Value > 0)
        {
            var a = new DoubleAnimation
            {
                From = _pitchRotation.Angle - pitchAmp.Value,
                To = _pitchRotation.Angle + pitchAmp.Value,
                Duration = new Duration(TimeSpan.FromSeconds(pitchPeriod.Value / 2.0)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            _pitchRotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, a);
        }

        var rollAmp = animCfg?.RollAmplitude;
        var rollPeriod = animCfg?.RollPeriodSec;
        if (_rollRotation != null && rollAmp.HasValue && rollPeriod.HasValue && rollPeriod.Value > 0)
        {
            var a = new DoubleAnimation
            {
                From = _rollRotation.Angle - rollAmp.Value,
                To = _rollRotation.Angle + rollAmp.Value,
                Duration = new Duration(TimeSpan.FromSeconds(rollPeriod.Value / 2.0)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            _rollRotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, a);
        }
    }
    
}
