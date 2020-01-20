using MahApps.Metro.Controls;
using Minimal_CS_Manga_Reader.ViewModel;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Minimal_CS_Manga_Reader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainView ViewModel { get; private set; }

        public MainWindow()
        {
            ViewModel = new MainView();
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