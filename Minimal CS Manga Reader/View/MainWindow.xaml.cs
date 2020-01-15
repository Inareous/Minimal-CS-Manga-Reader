using MahApps.Metro.Controls;
using Minimal_CS_Manga_Reader.ViewModel;
using System;
using System.Reactive.Linq;
using System.Windows.Controls;

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
            Scrollviewer.Events().ScrollChanged.Subscribe(x =>
            {
                ViewModel._scrollHeight = Scrollviewer.VerticalOffset.Equals(double.NaN) ? 0 : Scrollviewer.VerticalOffset;
                ViewModel.ScrollChanged();
            });
        }
    }
}