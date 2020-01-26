using MahApps.Metro.Controls;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Minimal_CS_Manga_Reader
{
    public partial class MainWindow : MetroWindow
    {
        public AppViewModel ViewModel { get; private set; }
        public MainWindow()
        {
            ViewModel = new AppViewModel();
            InitializeComponent();
            DataContext = ViewModel;

            ScrollViewer.Focus();

            ScrollViewer.Events().ScrollChanged.Subscribe(x =>
            {
                ViewModel._scrollHeight = ScrollViewer.VerticalOffset.Equals(double.NaN) ? 0 : ScrollViewer.VerticalOffset;
                ViewModel.ScrollChanged();
            });

            this.WhenAnyValue(x => x.ViewModel.ActiveIndex)
                .Subscribe(_ =>
                {
                    Keyboard.Focus(ScrollViewer);
                    ScrollViewer.Focus();
                });

            this.Events().KeyDown.
                Where(x => x.Key.Equals(Key.Enter)).
                Subscribe(x =>
                {
                    x.Handled = true;
                    var focusedControl = Keyboard.FocusedElement as System.Windows.FrameworkElement;
                    if (focusedControl is TextBox)
                    {
                        var expression = focusedControl.GetBindingExpression(TextBox.TextProperty);
                        expression?.UpdateSource();
                    }
                    ScrollViewer.Focus();
                });

            this.Events().KeyDown.
                Where(x => x.Key.Equals(Key.Insert)).
                Subscribe(x =>
                {
                    x.Handled = true;
                    ViewModel.PreviousClick.Execute().Subscribe();
                });

            this.Events().KeyDown.
                Where(x => x.Key.Equals(Key.Delete)).
                Subscribe(x =>
                {
                    x.Handled = true;
                    ViewModel.NextClick.Execute().Subscribe();
                });
        }
    }
}
