using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using Minimal_CS_Manga_Reader.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Minimal_CS_Manga_Reader
{
    public partial class MainWindow : MetroWindow, IViewFor<AppViewModel>
    {
        [Reactive] public AppViewModel ViewModel { get; set; } = Locator.Current.GetService<AppViewModel>();

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AppViewModel)value;
        }

        private System.Timers.Timer ChapterComboBoxCooldownTimer;

        private bool _isChapterComboBoxOnCooldown = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ScrollViewer.Focus();

            ScrollViewer.Events().ScrollChanged.Subscribe(_ =>
            {
                ViewModel._scrollHeight = ScrollViewer.VerticalOffset.Equals(double.NaN) ? 0 : ScrollViewer.VerticalOffset;
                ViewModel.ScrollChanged();
            });

            ScrollViewer.WhenAnyValue(x => x.ViewportWidth).Subscribe(x =>
            {
                // To fix : This part got hit twice when changing to fullscreen
                ViewModel.ViewportWidth = ViewModel.Config.FitImagesToScreen ? x : int.MaxValue;
            });

            ChapterComboBox.Events().SelectionChanged.Subscribe(_ =>
            {
                ViewModel._activeIndex = ChapterComboBox.SelectedIndex;
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
                        WindowStyle = WindowStyle.None;
                        IgnoreTaskbarOnMaximize = true;
                        ShowTitleBar = false;
                        ShowCloseButton = false;
                        ViewModel.ToolbarHeight = 0;
                        ResizeMode = ResizeMode.NoResize;

                        WindowState = WindowState.Maximized;
                    }
                    else
                    {
                        WindowStyle = WindowStyle.SingleBorderWindow;
                        IgnoreTaskbarOnMaximize = false;
                        ShowTitleBar = true;
                        ShowCloseButton = true;
                        ViewModel.ToolbarHeight = 30;
                        ResizeMode = ResizeMode.CanResize;
                    }
                });

            this.Events().KeyDown
                .Where(x => x.Key.Equals(Key.F5))
                .Subscribe(x =>
                {
                    x.Handled = true;
                    ViewModel.IsFullscreen = !ViewModel.IsFullscreen;
                });

            this.Events().KeyDown
                .Where(x => x.Key.Equals(Key.F2))
                .Subscribe(x =>
                {
                    x.Handled = true;
                    var cDialog = DialogCoordinator.Instance.GetCurrentDialogAsync<CustomDialog>(this).Result;
                    if (cDialog == null) ViewModel.OpenBookmark.Execute().Subscribe();
                    else if (cDialog.Name == "Bookmarks") DialogCoordinator.Instance.HideMetroDialogAsync(this, dlg);
                });

            this.Events().KeyDown
                .Where(x => x.Key.Equals(Key.Escape) && ViewModel.IsFullscreen)
                .Subscribe(x =>
                {
                    x.Handled = true;
                    ViewModel.IsFullscreen = !ViewModel.IsFullscreen;
                });

            this.Events().KeyDown
                .Where(x => x.Key.Equals(Key.Enter))
                .Subscribe(x =>
                {
                    x.Handled = true;
                    var focusedControl = Keyboard.FocusedElement as FrameworkElement;
                    if (focusedControl is TextBox)
                    {
                        var expression = focusedControl.GetBindingExpression(TextBox.TextProperty);
                        expression?.UpdateSource();
                    }
                    ScrollViewer.Focus();
                });

            this.Events().KeyDown
                .Where(x => x.Key.Equals(Key.Insert))
                .Select(args => System.Reactive.Unit.Default)
                .InvokeCommand(ViewModel.PreviousClick);

            this.Events().KeyDown
                .Where(x => x.Key.Equals(Key.Delete))
                .Select(args => System.Reactive.Unit.Default)
                .InvokeCommand(ViewModel.NextClick);

            this.Events().KeyDown
                .Where(x => x.Key.Equals(Key.F1))
                .Subscribe(x =>
                {
                    x.Handled = true;
                    if (ViewModel.AddBookmark())
                    {
                        BookmarkSnackBar.Invoke(async () =>
                        {
                            // Manage message manually instead of using MessageQueue to avoid thread blocking with updating image
                            BookmarkSnackBar.Message = new MaterialDesignThemes.Wpf.SnackbarMessage
                            {
                                Content = new Label //Doesn't work with TextBlock
                                {
                                    Content = "Bookmark Added!",
                                    FontWeight = FontWeights.SemiBold,
                                    FontSize = 13,
                                    Margin = new Thickness(0),
                                    Padding = new Thickness(0)
                                }
                            };
                            BookmarkSnackBar.IsActive = true;
                            await System.Threading.Tasks.Task.Delay(1000);
                            BookmarkSnackBar.IsActive = false;
                        }).ConfigureAwait(false);
                    }
                });

            this.WhenActivated(d =>
            {
                new DialogParticipationRegistration(this).DisposeWith(d);

                ViewModel.SettingDialogInteraction.RegisterHandler(async interaction =>
                {
                    dlg = GetCustomDialog();
                    dlg.Name = "Settings";

                    var dlgvm = new SettingViewModel((SettingViewModel vm, bool IsSaved) =>
                    {
                        DialogCoordinator.Instance.HideMetroDialogAsync(this, dlg);
                        interaction.SetOutput(IsSaved);
                    }, ViewModel.Config);

                    dlg.Content = new ViewModelViewHost { ViewModel = dlgvm };
                    dlg.Background = System.Windows.Media.Brushes.Transparent;
                    dlg.HorizontalAlignment = HorizontalAlignment.Center;

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

                ViewModel.BookmarkDialogInteraction.RegisterHandler(async interaction =>
                {
                    dlg = GetCustomDialog();
                    dlg.Name = "Bookmarks";

                    var dlgvm = new BookmarkViewModel((BookmarkViewModel vm, Bookmark bookmarkItem) =>
                    {
                        DialogCoordinator.Instance.HideMetroDialogAsync(this, dlg);
                        interaction.SetOutput(bookmarkItem);
                    }, ViewModel.BookmarksSource);

                    dlg.Content = new ViewModelViewHost { ViewModel = dlgvm };
                    dlg.Background = System.Windows.Media.Brushes.Transparent;
                    dlg.HorizontalAlignment = HorizontalAlignment.Center;

                    await DialogCoordinator.Instance.ShowMetroDialogAsync(this, dlg);

                    await dlg.WaitUntilUnloadedAsync();
                }).DisposeWith(d);
            });
        }

        private CustomDialog dlg; 

        private CustomDialog GetCustomDialog()
        {
            if (Locator.Current.GetService<CustomDialog>() == null)
            {
                var metroDialogSettings = new MetroDialogSettings()
                {
                    AnimateHide = false,
                    AnimateShow = true,
                };
                Locator.CurrentMutable.Register(() => new CustomDialog(metroDialogSettings));
            };
            return Locator.Current.GetService<CustomDialog>();
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