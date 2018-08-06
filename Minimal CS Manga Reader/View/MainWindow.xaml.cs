using Minimal_CS_Manga_Reader.ViewModel;
using System.Windows;

namespace Minimal_CS_Manga_Reader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainView ViewModel { get; private set; }

        public MainWindow()
        {
            ViewModel = new MainView();

            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}