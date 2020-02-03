using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows;
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
            });
        }

        public SettingViewModel ViewModel { get; set; }
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (SettingViewModel)value;
        }
    }
}
