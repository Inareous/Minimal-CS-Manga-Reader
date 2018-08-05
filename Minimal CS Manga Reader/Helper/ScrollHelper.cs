using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;


namespace Minimal_CS_Manga_Reader.Helper
{
    public class
            ScrollHelper : DependencyObject // Introducting mem leaks, read here : http://matthamilton.net/touchscrolling-for-scrollviewer
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ScrollHelper),
                new UIPropertyMetadata(false, IsEnabledChanged));

        private static readonly Dictionary<object, MouseCapture> _captures = new Dictionary<object, MouseCapture>();

        private static ScrollViewer Obj;

        public static double DragScaler { get; set; } = 3.5; // 0~10 for safe range

        public bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        private static void IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (ScrollViewer)d;
            if (target == null) return;

            if ((bool)e.NewValue)
                target.Loaded += target_Loaded;
            else
                target_Unloaded(target, new RoutedEventArgs());
        }

        private static void target_Unloaded(object sender, RoutedEventArgs e)
        {
            var target = (ScrollViewer)sender;
            if (target == null) return;
            _captures.Remove(sender);

            target.Loaded -= target_Loaded;
            target.Unloaded -= target_Unloaded;
            target.PreviewMouseLeftButtonDown -= target_PreviewMouseLeftButtonDown;
            target.PreviewMouseMove -= target_PreviewMouseMove;
            target.PreviewMouseLeftButtonUp -= target_PreviewMouseLeftButtonUp;
            target.PreviewKeyDown -= target_KeyDown;
            target.ScrollChanged -= target_ScrollChanged;
        }

        private static void target_Loaded(object sender, RoutedEventArgs e)
        {
            var target = (ScrollViewer)sender;
            Obj = target;
            if (target == null) return;
            target.Unloaded += target_Unloaded;
            target.PreviewMouseLeftButtonDown += target_PreviewMouseLeftButtonDown;
            target.PreviewMouseMove += target_PreviewMouseMove;
            target.PreviewMouseLeftButtonUp += target_PreviewMouseLeftButtonUp;
            target.PreviewKeyDown += target_KeyDown;
            target.ScrollChanged += target_ScrollChanged;
        }

        public static void Helper()
        {
            if (Obj == null) return;
            Obj.ScrollToHorizontalOffset(Math.Round(Obj.ScrollableWidth / 2 - 1));
            Obj.ScrollToTop();
        }

        private static void target_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var target = (ScrollViewer)sender;
            if (target == null) return;
            if (!target.IsMouseOver) return;
            if (!target.IsFocused) return;

            _captures[sender] = new MouseCapture
            {
                VerticalOffset = target.VerticalOffset,
                Point = e.GetPosition(target)
            };
            target.Cursor = Cursors.SizeNS;
        }

        public static void target_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Remove "|| e.ViewportHeightChange < 0 || e.ExtentHeightChange < 0" if you want it to only scroll to the bottom when it increases in size
            if (!(e.ViewportHeightChange > 0) && !(e.ExtentHeightChange > 0) && !(e.ViewportHeightChange < 0) &&
                !(e.ExtentHeightChange < 0)) return;
            var x = sender as ScrollViewer;
            // x?.ScrollToTop();
            x?.ScrollToHorizontalOffset(Math.Round(x.ScrollableWidth / 2 - 1));
        }

        private static void target_KeyDown(object sender, KeyEventArgs e)
        {
            var target = (ScrollViewer)sender;
            switch (e.Key)
            {
                case Key.PageUp:
                    target.ScrollToVerticalOffset(Math.Round(target.VerticalOffset - target.ViewportHeight * 4 / 5));
                    e.Handled = true;
                    break;
                case Key.PageDown:
                    target.ScrollToVerticalOffset(Math.Round(target.VerticalOffset + target.ViewportHeight * 4 / 5));
                    e.Handled = true;
                    break;
                case Key.Up:
                    target.ScrollToVerticalOffset(Math.Round(target.VerticalOffset - target.ViewportHeight * 1 / 10));
                    e.Handled = true;
                    break;
                case Key.Down:
                    target.ScrollToVerticalOffset(Math.Round(target.VerticalOffset + target.ViewportHeight * 1 / 10));
                    e.Handled = true;
                    break;
                case Key.Left:
                    target.ScrollToVerticalOffset(Math.Round(target.VerticalOffset - target.ViewportHeight * 4 / 5));
                    e.Handled = true;
                    break;
                case Key.Right:
                    target.ScrollToVerticalOffset(Math.Round(target.VerticalOffset + target.ViewportHeight * 4 / 5));
                    e.Handled = true;
                    break;
                case Key.Home:
                    target.ScrollToHome();
                    break;
                case Key.End:
                    target.ScrollToBottom();
                    break;
            }
        }

        private static void target_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var target = (ScrollViewer)sender;
            target.Cursor = Cursors.Arrow;
            target?.ReleaseMouseCapture();
        }


        private static void target_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var original = e.OriginalSource;
            if (FindVisualParent<ScrollBar>(original as DependencyObject) != null) return;
            if (!_captures.ContainsKey(sender)) return;

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _captures.Remove(sender);
                return;
            }

            var target = (ScrollViewer)sender;


            var capture = _captures[sender];

            var point = e.GetPosition(target);

            var dy = (point.Y - capture.Point.Y) * DragScaler;
            var dx = (point.X - capture.Point.X)*DragScaler;
            if (Math.Abs(dy) > 5) target.CaptureMouse();

            target.ScrollToVerticalOffset(Math.Round(capture.VerticalOffset - dy));
            target.ScrollToHorizontalOffset(Math.Round(capture.HorizontalOffset - dx));
        }

        private static TParentItem FindVisualParent<TParentItem>(DependencyObject obj)
            where TParentItem : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(obj);
            while (parent != null && parent.GetType() != typeof(TParentItem))
                parent = VisualTreeHelper.GetParent(parent);
            return parent as TParentItem;
        }

        internal class MouseCapture
        {
            public double VerticalOffset { get; set; }
            public Point Point { get; set; }
            public double HorizontalOffset { get; set; }
        }
    }
}
