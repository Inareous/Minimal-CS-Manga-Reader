using ReactiveUI;
using Splat;
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
            Locator.CurrentMutable.Register(() => new SettingView(), typeof(IViewFor<SettingViewModel>));
            _ = Task.Run(() => DataSource.Initialize(System.Environment.GetCommandLineArgs()));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }
}