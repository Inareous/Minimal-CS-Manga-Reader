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
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Task.Run(() => {
                var _args = System.Environment.GetCommandLineArgs();
                DataSource.Initialize(_args);
            }).ConfigureAwait(false);
            base.OnStartup(e);
        }
    }
}