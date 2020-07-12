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
        readonly IDataSource data;
        readonly IBookmarksSource bookmarks;

        public App()
        {
            Locator.CurrentMutable.RegisterConstant(new DataSource(), typeof(IDataSource));
            Locator.CurrentMutable.RegisterConstant(new BookmarksSource(), typeof(IBookmarksSource));
            //
            Locator.CurrentMutable.Register(() => new AppViewModel(Locator.Current.GetService<IDataSource>(), Locator.Current.GetService<IBookmarksSource>()));
            Locator.CurrentMutable.Register(() => new MainWindow(), typeof(IViewFor<AppViewModel>));
            Locator.CurrentMutable.RegisterLazySingleton(() => new BookmarkView(), typeof(IViewFor<BookmarkViewModel>));
            Locator.CurrentMutable.RegisterLazySingleton(() => new SettingView(), typeof(IViewFor<SettingViewModel>));
            //
            data = Locator.Current.GetService<IDataSource>();
            bookmarks = Locator.Current.GetService<IBookmarksSource>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            data.InitializeAsync(System.Environment.GetCommandLineArgs());
            bookmarks.LoadAsync();
            Current.MainWindow = (MainWindow)Locator.Current.GetService(typeof(IViewFor<AppViewModel>));
            Current.MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            bookmarks.SaveAsync();
        }
    }
}