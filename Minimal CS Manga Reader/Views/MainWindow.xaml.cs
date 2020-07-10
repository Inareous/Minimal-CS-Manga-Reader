using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using System;
using Microsoft.WindowsAPICodePack.Dialogs;
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

        private System.Timers.Timer ChapterComboBoxCooldownTimer;

        private bool _isChapterComboBoxOnCooldown = false;

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

            ScrollViewer.WhenAnyValue(x => x.ViewportWidth).Subscribe(x=>
            {
                // To fix : This part got hit twice when changing to fullscreen
                ViewModel.ViewportWidth = Settings.Default.FitImagesToScreen ? ScrollViewer.ViewportWidth : int.MaxValue;
            });

            ChapterComboBox.Events().MouseWheel.Subscribe(x =>
            {
                if (!_isChapterComboBoxOnCooldown)
                {
                    SetComboBoxCooldown();
                    if (x.Delta >= 120) ViewModel.PreviousClick.Execute().Subscribe();
                    else if (x.Delta <= -120) ViewModel.NextClick.Execute().Subscribe();
                }
            });

            ActiveIndexBox.Events().LostKeyboardFocus.Subscribe(_ =>
            {
                var validInput = int.TryParse(ActiveIndexBox.Text, out int newVal);
                if (validInput && newVal != ViewModel.ActiveImage && newVal > 0 && newVal <= ViewModel.ImageHeight.Count)
                {
                    ScrollViewer.ScrollToVerticalOffset(ViewModel.ImageHeight[newVal-1] - ViewModel.ImageDimension[newVal-1].Item2 - int.Parse(ViewModel.ImageMarginSetter) + 0.1);
                }
                this.OneWayBind(ViewModel, vm => vm.ActiveImage, view => view.ActiveIndexBox.Text);
            });

            this.WhenAnyValue(x => x.ViewModel.ActiveIndex)
                .Subscribe(_ =>
                {
                    Keyboard.Focus(ScrollViewer);
                    ScrollViewer.Focus();
                });

            this.WhenAnyValue(x => x.ViewModel.IsFullscreen)
                .Subscribe(x =>
                {
                    if (x)
                    {
                        WindowStyle = System.Windows.WindowStyle.None;
                        IgnoreTaskbarOnMaximize = true;
                        ShowTitleBar = false;
                        ShowCloseButton = false;
                        ViewModel.ToolbarHeight = 0;
                        ResizeMode = System.Windows.ResizeMode.NoResize;

                        WindowState = System.Windows.WindowState.Maximized;
                    }
                    else
                    {
                        WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                        IgnoreTaskbarOnMaximize = false;
                        ShowTitleBar = true;
                        ShowCloseButton = true;
                        ViewModel.ToolbarHeight = 30;
                        ResizeMode = System.Windows.ResizeMode.CanResize;
                    }
                });

            this.Events().KeyDown.
                Where(x => x.Key.Equals(Key.F5)).
                Subscribe(x =>
                {
                    x.Handled = true;
                    ViewModel.IsFullscreen = !ViewModel.IsFullscreen;
                });

            this.Events().KeyDown.
                Where(x => x.Key.Equals(Key.Escape) && ViewModel.IsFullscreen).
                Subscribe(x =>
                {
                    x.Handled = true;
                    ViewModel.IsFullscreen = !ViewModel.IsFullscreen;
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

                ViewModel.FolderDialogInteraction.RegisterHandler(interaction =>
                {
                    CommonOpenFileDialog dialog = new CommonOpenFileDialog
                    {
                        InitialDirectory = interaction.Input,
                        IsFolderPicker = true
                    };
                    var callback = dialog.ShowDialog();

                    if (callback == CommonFileDialogResult.Ok)
                    {
                        interaction.SetOutput((callback, dialog.FileName));
                    }
                    else
                    {
                        interaction.SetOutput((callback, ""));
                    }
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

        private void SetComboBoxCooldown()
        {
            _isChapterComboBoxOnCooldown = true;
            ChapterComboBoxCooldownTimer = new System.Timers.Timer(100); // Arbitrary, 100ms is good enough to stop incidental massive scroll
            ChapterComboBoxCooldownTimer.Elapsed += (_, err) => _isChapterComboBoxOnCooldown = false;
            ChapterComboBoxCooldownTimer.AutoReset = false;
            ChapterComboBoxCooldownTimer.Start();
        }
    }
}