using Minimal_CS_Manga_Reader.Models;
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
            Locator.CurrentMutable.RegisterLazySingleton(() => new BookmarkView(), typeof(IViewFor<BookmarkViewModel>));
            Locator.CurrentMutable.RegisterLazySingleton(() => new SettingView(), typeof(IViewFor<SettingViewModel>));
            _ = DataSource.InitializeAsync(System.Environment.GetCommandLineArgs());
            _ = BookmarksSource.LoadAsync();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _ = BookmarksSource.SaveAsync();
        }
    }
}