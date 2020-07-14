using DynamicData;
using Minimal_CS_Manga_Reader.Helper;
using Minimal_CS_Manga_Reader.Models;
using Splat;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Minimal_CS_Manga_Reader
{
    public class DataSource : IDataSource
    {
        #region Property

        private readonly IUserConfig Config;

        private DataCollector collector;
        public string Title { get; private set; } = "";

        public SourceList<Entry> ChapterList { get; private set; } = new SourceList<Entry>();
        public string Path { get; private set; } = "";

        public string ActiveChapterPath { get; set; } = System.IO.Path.DirectorySeparatorChar.ToString();

        public SourceList<BitmapSource> ImageList { get; private set; } = new SourceList<BitmapSource>();

        #endregion Property

        #region Method

        public DataSource(IUserConfig config)
        {
            Config = config ?? Locator.Current.GetService<IUserConfig>();
            collector = new DataCollector(Config);
        }

        public async Task InitializeAsync(string[] args)
        {
            Path = Config.Path;
            if (args.Length > 1)
            {
                for (int i = 1; i < args.Length; i++)
                {
                    var argument = args[i];
                    if (argument.Contains("-path=")) argument = argument.Replace("-path=", ""); // Relic from old context integrate (maintain compatibility for now) :(
                    if (PathHelper.EnsureValidPath(argument)) Path = argument;
                }
            }

            if (Path == "FirstTimeNotSet")
            {
                Path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }

            await SetChapter(Path);
        }

        public async Task SetChapter(string path)
        {
            bool IsPathArchiveFile;

            if (Directory.Exists(path)) IsPathArchiveFile = false;
            else if (PathHelper.EnsureValidArchives(path)) IsPathArchiveFile = true;
            else return;

            Path = path;
            Config.Path = Path;
            Title = Path.Split(System.IO.Path.DirectorySeparatorChar).ToArray()[^1];
            ChapterList.Clear();
            var list = await collector.GetChapterListAsync(Path, IsPathArchiveFile);
            ChapterList.AddRange(list);
        }

        public async Task PopulateImageAsync(Entry entry, CancellationToken token)
        {
            ActiveChapterPath = entry.AbsolutePath;

            if (ImageList.Count > 0)
            {
                ImageList.Clear();
            }

            try
            {
                await Task.Run(() => collector.GetImages(ActiveChapterPath, ImageList, token)).ConfigureAwait(false);
            }
            catch (System.OperationCanceledException)
            {
                ImageList.Clear();
            }
        }

        #endregion Method
    }
}