using DynamicData;
using Minimal_CS_Manga_Reader.Helper;
using Minimal_CS_Manga_Reader.Models;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Minimal_CS_Manga_Reader
{
    public class DataSource
    {
        #region Property

        private static readonly DataCollector collector = new DataCollector();
        public static string Title { get; private set; } = "";


        public static SourceList<Entry> ChapterList { get; private set; } = new SourceList<Entry>();
        public static string Path { get; private set; } = Settings.Default.Path;

        public static string ActiveChapterPath { get; set; } = System.IO.Path.DirectorySeparatorChar.ToString();

        public static SourceList<BitmapSource> ImageList { get; private set; } = new SourceList<BitmapSource>();

        #endregion Property

        #region Method

        public static async Task InitializeAsync(string[] args)
        {
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

        public static async Task SetChapter(string path)
        {
            bool IsPathArchiveFile;

            if (Directory.Exists(path)) IsPathArchiveFile = false;
            else if (PathHelper.EnsureValidArchives(path)) IsPathArchiveFile = true;
            else return;

            Path = path;
            Settings.Default.Path = Path;
            Settings.Default.Save();
            Title = Path.Split(System.IO.Path.DirectorySeparatorChar).ToArray()[^1];
            ChapterList.Clear();
            var list = await collector.GetChapterListAsync(Path, IsPathArchiveFile);
            ChapterList.AddRange(list);
        }

        public static async Task PopulateImageAsync(CancellationToken token)
        {
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