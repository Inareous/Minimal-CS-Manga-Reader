using System.Threading.Tasks;
using System.Windows;

namespace Minimal_CS_Manga_Reader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            _ = Task.Run(() => DataSource.Initialize(System.Environment.GetCommandLineArgs()));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
           base.OnStartup(e);
        }
    }
}