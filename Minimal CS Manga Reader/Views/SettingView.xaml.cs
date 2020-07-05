using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Disposables;
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
                this.Bind(ViewModel, vm => vm.SelectedPixelOffsetMode, view => view.PixelOffsetMode.SelectedItem).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PixelOffsetMode, view => view.PixelOffsetMode.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedInterpolationMode, view => view.InterpolationMode.SelectedItem).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.InterpolationMode, view => view.InterpolationMode.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedSmoothingMode, view => view.SmoothingMode.SelectedItem).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.SmoothingMode, view => view.SmoothingMode.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ContextIntegrated, view => view.ContextIntegrated.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.FitImagesToScreen, view => view.FitImagesToScreen.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.OpenChapterOnLoad, view => view.OpenChapterOnLoadList.SelectedItem).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.OpenChapterOnLoadList, view => view.OpenChapterOnLoadList.ItemsSource).DisposeWith(d);
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