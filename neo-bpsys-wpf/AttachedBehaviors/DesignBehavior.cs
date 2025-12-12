using neo_bpsys_wpf.Controls;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;

namespace neo_bpsys_wpf.AttachedBehaviors;

    public static class DesignBehavior
    {
        private const double ZeroThreshold = 2.0;
        private const double UpdateThreshold = 3.0;
        public static readonly DependencyProperty IsDesignModeProperty =
            DependencyProperty.RegisterAttached("IsDesignMode", typeof(bool), typeof(DesignBehavior),
                new PropertyMetadata(false, OnIsDesignModeChanged));

        public static readonly DependencyProperty LastKnownLeftProperty =
            DependencyProperty.RegisterAttached("LastKnownLeft", typeof(double), typeof(DesignBehavior),
                new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty LastKnownTopProperty =
            DependencyProperty.RegisterAttached("LastKnownTop", typeof(double), typeof(DesignBehavior),
                new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty PositionGuardHandlerProperty =
            DependencyProperty.RegisterAttached("PositionGuardHandler", typeof(EventHandler), typeof(DesignBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CanvasPositionGuardHandlerProperty =
            DependencyProperty.RegisterAttached("CanvasPositionGuardHandler", typeof(EventHandler), typeof(DesignBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty LeftChangedHandlerProperty =
            DependencyProperty.RegisterAttached("LeftChangedHandler", typeof(EventHandler), typeof(DesignBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TopChangedHandlerProperty =
            DependencyProperty.RegisterAttached("TopChangedHandler", typeof(EventHandler), typeof(DesignBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CanvasFrozenProperty =
            DependencyProperty.RegisterAttached("CanvasFrozen", typeof(bool), typeof(DesignBehavior),
                new PropertyMetadata(false));

    public static bool GetIsDesignMode(UIElement element)
    {
        return (bool)element.GetValue(IsDesignModeProperty);
    }

    public static void SetIsDesignMode(UIElement element, bool value)
    {
        element.SetValue(IsDesignModeProperty, value);
    }

        private static void OnIsDesignModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if ((bool)e.NewValue)
                {
                    try
                    {
                        FreezeCanvasPositionIfUnset(element);
                        FreezeAllChildrenPositions(element);
                        StartPositionGuard(element);
                        var myAdornerLayer = AdornerLayer.GetAdornerLayer(element);
                        if (myAdornerLayer != null)
                        {
                            myAdornerLayer.Add(new CanvasAdorner(element));
                        }
                        else
                        {
                            RoutedEventHandler? handler = null;
                            handler = (s, e2) =>
                            {
                                element.Loaded -= handler;
                                var layer = AdornerLayer.GetAdornerLayer(element);
                                if (layer != null)
                                {
                                    layer.Add(new CanvasAdorner(element));
                                }
                            };
                            element.Loaded += handler;
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    try
                    {
                        StopPositionGuard(element);
                        var adornerLayer = AdornerLayer.GetAdornerLayer(element);
                        var adorners = adornerLayer?.GetAdorners(element);
                        if (adorners == null) return;
                        foreach (var adorner in adorners)
                        {
                            adornerLayer?.Remove(adorner);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

    private static Canvas? FindParentCanvas(FrameworkElement element)
    {
        DependencyObject? parent = element;
        while (parent != null && parent is not Canvas)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }
        return parent as Canvas;
    }

        private static void FreezeCanvasPositionIfUnset(FrameworkElement element)
        {
            var canvas = FindParentCanvas(element);
            if (canvas == null) return;

            void Apply()
            {
                var left = Canvas.GetLeft(element);
                var top = Canvas.GetTop(element);
                try
                {
                    var p = element.TransformToAncestor(canvas).Transform(new Point(0, 0));
                    var l = p.X - element.Margin.Left;
                    var t = p.Y - element.Margin.Top;
                    if (element.RenderTransform is TranslateTransform tt)
                    {
                        l -= tt.X;
                        t -= tt.Y;
                    }
                    if (double.IsNaN(left)) Canvas.SetLeft(element, l);
                    if (double.IsNaN(top)) Canvas.SetTop(element, t);
                }
                catch
                {
                }
            }

            if (element.IsLoaded)
            {
                Apply();
            }
            else
            {
                RoutedEventHandler? handler = null;
                handler = (s, e) =>
                {
                    element.Loaded -= handler;
                    Apply();
                };
                element.Loaded += handler;
            }
        }

        private static void FreezeAllChildrenPositions(FrameworkElement element)
        {
            var canvas = FindParentCanvas(element);
            if (canvas == null) return;

            var frozen = (bool)canvas.GetValue(CanvasFrozenProperty);
            if (frozen) return;

            foreach (var child in canvas.Children)
            {
                if (child is not FrameworkElement fe) continue;
                var left = Canvas.GetLeft(fe);
                var top = Canvas.GetTop(fe);
                if (double.IsNaN(left) || double.IsNaN(top))
                {
                    try
                    {
                        var p = fe.TransformToAncestor(canvas).Transform(new Point(0, 0));
                        var l = p.X - fe.Margin.Left;
                        var t = p.Y - fe.Margin.Top;
                        if (fe.RenderTransform is TranslateTransform tt)
                        {
                            l -= tt.X;
                            t -= tt.Y;
                        }
                        if (double.IsNaN(left)) Canvas.SetLeft(fe, l);
                        if (double.IsNaN(top)) Canvas.SetTop(fe, t);
                    }
                    catch
                    {
                    }
                }
                var cl = Canvas.GetLeft(fe);
                var ct = Canvas.GetTop(fe);
                if (!double.IsNaN(cl)) fe.SetValue(LastKnownLeftProperty, cl);
                if (!double.IsNaN(ct)) fe.SetValue(LastKnownTopProperty, ct);
            }

            canvas.SetValue(CanvasFrozenProperty, true);
        }

    private static void StartPositionGuard(FrameworkElement element)
    {
        var canvas = FindParentCanvas(element);
        if (canvas == null) return;

        var left = Canvas.GetLeft(element);
        var top = Canvas.GetTop(element);
        if (!double.IsNaN(left)) element.SetValue(LastKnownLeftProperty, left);
        if (!double.IsNaN(top)) element.SetValue(LastKnownTopProperty, top);

        EventHandler? handler = null;
        handler = (s, _) =>
        {
            var cl = Canvas.GetLeft(element);
            var ct = Canvas.GetTop(element);
            if (double.IsNaN(cl) || double.IsNaN(ct))
            {
                var ll = (double)element.GetValue(LastKnownLeftProperty);
                var lt = (double)element.GetValue(LastKnownTopProperty);
                if (!double.IsNaN(ll)) Canvas.SetLeft(element, ll);
                if (!double.IsNaN(lt)) Canvas.SetTop(element, lt);
            }
            else
            {
                var ll = (double)element.GetValue(LastKnownLeftProperty);
                var lt = (double)element.GetValue(LastKnownTopProperty);
                var clNearZero = (cl >= -ZeroThreshold && cl <= ZeroThreshold);
                var ctNearZero = (ct >= -ZeroThreshold && ct <= ZeroThreshold);
                if (!clNearZero && (double.IsNaN(ll) || Math.Abs(cl - ll) >= UpdateThreshold)) element.SetValue(LastKnownLeftProperty, cl);
                if (!ctNearZero && (double.IsNaN(lt) || Math.Abs(ct - lt) >= UpdateThreshold)) element.SetValue(LastKnownTopProperty, ct);
            }
        };
        element.SetValue(PositionGuardHandlerProperty, handler);
        element.LayoutUpdated += handler;

        var leftDesc = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Canvas.LeftProperty, typeof(FrameworkElement));
        var topDesc = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Canvas.TopProperty, typeof(FrameworkElement));

        EventHandler? leftHandler = null;
        leftHandler = (s, _) =>
        {
            var cl = Canvas.GetLeft(element);
            var llObj = element.GetValue(LastKnownLeftProperty);
            var ll = llObj is double d ? d : double.NaN;
            var nearZero = (cl >= -ZeroThreshold && cl <= ZeroThreshold);
            var lastFar = !double.IsNaN(ll) && Math.Abs(ll) > ZeroThreshold;
            if (nearZero && lastFar)
            {
                Canvas.SetLeft(element, ll);
            }
            else if (!double.IsNaN(cl))
            {
                if (!nearZero && (double.IsNaN(ll) || Math.Abs(cl - ll) >= UpdateThreshold))
                    element.SetValue(LastKnownLeftProperty, cl);
            }
        };

        EventHandler? topHandler = null;
        topHandler = (s, _) =>
        {
            var ct = Canvas.GetTop(element);
            var ltObj = element.GetValue(LastKnownTopProperty);
            var lt = ltObj is double d ? d : double.NaN;
            var nearZero = (ct >= -ZeroThreshold && ct <= ZeroThreshold);
            var lastFar = !double.IsNaN(lt) && Math.Abs(lt) > ZeroThreshold;
            if (nearZero && lastFar)
            {
                Canvas.SetTop(element, lt);
            }
            else if (!double.IsNaN(ct))
            {
                if (!nearZero && (double.IsNaN(lt) || Math.Abs(ct - lt) >= UpdateThreshold))
                    element.SetValue(LastKnownTopProperty, ct);
            }
        };

        element.SetValue(LeftChangedHandlerProperty, leftHandler);
        element.SetValue(TopChangedHandlerProperty, topHandler);
        leftDesc?.AddValueChanged(element, leftHandler);
        topDesc?.AddValueChanged(element, topHandler);
    }

    private static void StopPositionGuard(FrameworkElement element)
    {
        var h = element.GetValue(PositionGuardHandlerProperty) as EventHandler;
        if (h != null)
        {
            element.LayoutUpdated -= h;
            element.ClearValue(PositionGuardHandlerProperty);
        }

        var leftDesc = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Canvas.LeftProperty, typeof(FrameworkElement));
        var topDesc = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Canvas.TopProperty, typeof(FrameworkElement));

        var lh = element.GetValue(LeftChangedHandlerProperty) as EventHandler;
        if (lh != null)
        {
            leftDesc?.RemoveValueChanged(element, lh);
            element.ClearValue(LeftChangedHandlerProperty);
        }

        var th = element.GetValue(TopChangedHandlerProperty) as EventHandler;
        if (th != null)
        {
            topDesc?.RemoveValueChanged(element, th);
            element.ClearValue(TopChangedHandlerProperty);
        }
    }

    private static void StartCanvasChildrenPositionGuard(FrameworkElement element)
    {
        var canvas = FindParentCanvas(element);
        if (canvas == null) return;

        EventHandler? handler = null;
        handler = (s, _) =>
        {
            foreach (var child in canvas.Children)
            {
                if (child is not FrameworkElement fe) continue;

                var cl = Canvas.GetLeft(fe);
                var ct = Canvas.GetTop(fe);
                var llObj = fe.GetValue(LastKnownLeftProperty);
                var ltObj = fe.GetValue(LastKnownTopProperty);
                var ll = llObj is double d1 ? d1 : double.NaN;
                var lt = ltObj is double d2 ? d2 : double.NaN;

                var clNaN = double.IsNaN(cl);
                var ctNaN = double.IsNaN(ct);

                var nearZero = (cl >= -ZeroThreshold && cl <= ZeroThreshold) && (ct >= -ZeroThreshold && ct <= ZeroThreshold);
                var lastFar = (!double.IsNaN(ll) && Math.Abs(ll) > ZeroThreshold) || (!double.IsNaN(lt) && Math.Abs(lt) > ZeroThreshold);
                var isZeroJump = !clNaN && !ctNaN && nearZero && lastFar;

                if (clNaN || ctNaN || isZeroJump)
                {
                    if (!double.IsNaN(ll)) Canvas.SetLeft(fe, ll);
                    if (!double.IsNaN(lt)) Canvas.SetTop(fe, lt);

                    if (double.IsNaN(ll) || double.IsNaN(lt))
                    {
                        try
                        {
                            var p = fe.TransformToAncestor(canvas).Transform(new Point(0, 0));
                            var l = p.X - fe.Margin.Left;
                            var t = p.Y - fe.Margin.Top;
                            if (fe.RenderTransform is TranslateTransform tt)
                            {
                                l -= tt.X;
                                t -= tt.Y;
                            }
                            if (double.IsNaN(cl)) Canvas.SetLeft(fe, l);
                            if (double.IsNaN(ct)) Canvas.SetTop(fe, t);
                            fe.SetValue(LastKnownLeftProperty, Canvas.GetLeft(fe));
                            fe.SetValue(LastKnownTopProperty, Canvas.GetTop(fe));
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    var clNearZero = (cl >= -ZeroThreshold && cl <= ZeroThreshold);
                    var ctNearZero = (ct >= -ZeroThreshold && ct <= ZeroThreshold);
                    if (!clNearZero && (double.IsNaN(ll) || Math.Abs(cl - ll) >= UpdateThreshold)) fe.SetValue(LastKnownLeftProperty, cl);
                    if (!ctNearZero && (double.IsNaN(lt) || Math.Abs(ct - lt) >= UpdateThreshold)) fe.SetValue(LastKnownTopProperty, ct);
                }
            }
        };

        var existing = canvas.GetValue(CanvasPositionGuardHandlerProperty) as EventHandler;
        if (existing == null)
        {
            canvas.SetValue(CanvasPositionGuardHandlerProperty, handler);
            canvas.LayoutUpdated += handler;
        }
    }

    private static void StopCanvasChildrenPositionGuard(FrameworkElement element)
    {
        var canvas = FindParentCanvas(element);
        if (canvas == null) return;
        var h = canvas.GetValue(CanvasPositionGuardHandlerProperty) as EventHandler;
        if (h != null)
        {
            canvas.LayoutUpdated -= h;
            canvas.ClearValue(CanvasPositionGuardHandlerProperty);
        }
    }
}
