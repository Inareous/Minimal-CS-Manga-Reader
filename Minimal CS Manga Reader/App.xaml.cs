using Minimal_CS_Manga_Reader.Models;
using ReactiveUI;
using Splat;
using System.Windows;

namespace Minimal_CS_Manga_Reader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        readonly IBookmarksSource bookmarks;
        readonly IUserConfig config;

        public App()
        {
            Locator.CurrentMutable.RegisterConstant(new BookmarksSource(), typeof(IBookmarksSource));
            bookmarks = Locator.Current.GetService<IBookmarksSource>();
            Locator.CurrentMutable.RegisterConstant(new UserConfig(), typeof(IUserConfig));
            config = Locator.Current.GetService<IUserConfig>();
            Locator.CurrentMutable.RegisterConstant(new DataSource(config), typeof(IDataSource));
            //
            Locator.CurrentMutable.RegisterLazySingleton(() => new AppViewModel(Locator.Current.GetService<IDataSource>(), Locator.Current.GetService<IBookmarksSource>(), Locator.Current.GetService<IUserConfig>()));
            Locator.CurrentMutable.RegisterLazySingleton(() => new MainWindow(), typeof(IViewFor<AppViewModel>));
            Locator.CurrentMutable.RegisterLazySingleton(() => new BookmarkView(), typeof(IViewFor<BookmarkViewModel>));
            Locator.CurrentMutable.RegisterLazySingleton(() => new SettingView(), typeof(IViewFor<SettingViewModel>));
            //
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            config.Load();
            bookmarks.LoadAsync();
            Current.MainWindow = (MainWindow)Locator.Current.GetService(typeof(IViewFor<AppViewModel>));
            Current.MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            config.Save();
            bookmarks.SaveAsync();
        }
    }
}