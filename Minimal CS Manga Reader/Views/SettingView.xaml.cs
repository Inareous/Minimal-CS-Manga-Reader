using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace Minimal_CS_Manga_Reader
{
    /// <summary>
    /// Interaction logic for SettingView.xaml
    /// </summary>
    public partial class SettingView : UserControl, IViewFor<SettingViewModel>
    {
        public SettingView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.Close, view => view.Close).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Save, view => view.Save).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveAndRefresh, view => view.SaveAndRefresh).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SetAccentColor, view => view.SetAccentColor).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.CancelAccentColor, view => view.CancelAccentColor).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ResetAccentColor, view => view.ResetAccentColor).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PixelOffsetMode, view => view.PixelOffsetMode.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.InterpolationMode, view => view.InterpolationMode.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.SmoothingMode, view => view.SmoothingMode.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.OpenChapterOnLoadList, view => view.OpenChapterOnLoadList.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.BackgroundViewList, view => view.BackgroundView.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ThemeList, view => view.Theme.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedPixelOffsetMode, view => view.PixelOffsetMode.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedInterpolationMode, view => view.InterpolationMode.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedSmoothingMode, view => view.SmoothingMode.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedBackground, view => view.BackgroundView.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedTheme, view => view.Theme.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.OpenChapterOnLoad, view => view.OpenChapterOnLoadList.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ContextIntegrated, view => view.ContextIntegrated.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.FitImagesToScreen, view => view.FitImagesToScreen.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.IsScrollBarVisible, view => view.IsScrollBarVisible.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedColor, view => view.ColorPicker.Color).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedColor, view => view.ColorTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedBrush, view => view.PopupToggleButton.Background).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedBrush, view => view.BrushColor.Fill).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.IsPopupOpen, view => view.Popup.IsPopupOpen).DisposeWith(d);

                Popup.WhenAnyValue(x => x.IsPopupOpen)
                     .Where(x => x == false)
                     .Subscribe(x =>
                     {
                         ViewModel.SelectedColor = ViewModel.InitialAccentColor;
                     });
            });
        }

        [Reactive] public SettingViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (SettingViewModel)value;
        }
    }
}