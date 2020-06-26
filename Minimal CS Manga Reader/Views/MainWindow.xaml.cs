using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Minimal_CS_Manga_Reader
{
    public partial class MainWindow : MetroWindow, IViewFor<AppViewModel>
    {
        public AppViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AppViewModel)value;
        }

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

            this.WhenActivated(d =>
            {
                new DialogParticipationRegistration(this).DisposeWith(d);

                ViewModel.SettingDialogInteraction.RegisterHandler(async interaction =>
                {
                    var metroDialogSettings = new MetroDialogSettings()
                    {
                        AnimateHide = false,
                        AnimateShow = true,
                    };
                    var dlg = new CustomDialog(metroDialogSettings);

                    var dlgvm = new SettingViewModel((SettingViewModel vm, bool IsSaved) =>
                    {
                        DialogCoordinator.Instance.HideMetroDialogAsync(this, dlg);
                        interaction.SetOutput(IsSaved);
                    });

                    dlg.Content = new ViewModelViewHost { ViewModel = dlgvm };
                    dlg.Background = System.Windows.Media.Brushes.Transparent;
                    dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

                    await DialogCoordinator.Instance.ShowMetroDialogAsync(this, dlg);

                    await dlg.WaitUntilUnloadedAsync();
                }).DisposeWith(d);
            });
        }

        private class DialogParticipationRegistration : IDisposable
        {
            private readonly MainWindow _view;

            public DialogParticipationRegistration(MainWindow view)
            {
                _view = view;
                DialogParticipation.SetRegister(view, view);
            }

            public void Dispose()
            {
                DialogParticipation.SetRegister(_view, null);
            }
        }
    }
}