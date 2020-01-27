using DynamicData;
using ReactiveUI;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Minimal_CS_Manga_Reader
{
    public class DataSource : ReactiveObject
    {
        #region Property
        private static readonly DataCollector collector = new DataCollector();
        public static string Title { get; private set; } = "";

        private static bool IsArgsPathArchiveFile { get; set; } = false;

        public static List<string> ChapterList { get; private set; } = new List<string>();
        public static string Path { get; private set; } = Settings.Default.Path;

        public static string ActiveChapterPath { get; set; }

        public static SourceList<BitmapSource> ImageList { get; private set; } = new SourceList<BitmapSource>();
        #endregion

        #region Method
        public static void Initialize(string[] args)
        {
            if (Path == "FirstTimeNotSet")
            {
                Path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }

            if (args.Length >= 2)
            {
                foreach (var argument in args)
                {
                    if (argument.Contains("-path="))
                    {
                        var ArgPath = argument.Replace("-path=", "");
                        ConfigureProgramBasedOnPath(ArgPath);
                        break;
                    }
                }
            }
        }

        private static void ConfigureProgramBasedOnPath(string path)
        {
            if (Directory.Exists(path))
            {
                IsArgsPathArchiveFile = false;
            }
            else if (ZipArchive.IsZipFile(path) || RarArchive.IsRarFile(path))
            {
                IsArgsPathArchiveFile = true;
            }
            else
            {
                return;
            }
            Path = path;
            Settings.Default.Path = Path;
            Settings.Default.Save();
            Title = Path.Split('\\').ToArray()[^1];
            _ = Task.Run(() =>
            {
                ChapterList = collector.GetChapterListAsync(Path, IsArgsPathArchiveFile).Result.ToList();
                ActiveChapterPath = ChapterList.Count != 0 ? ChapterList[^1] : "\\";
            });
        }

        public static async Task PopulateImageAsync(CancellationToken token)
        {
            if (ImageList.Count > 0)
            {
                ImageList.Clear();
            }

            await Task.Run(() =>
            {
                var T = Task.Run(() => collector.GetImagesAsync(ActiveChapterPath, ImageList, token), token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                return T;
            });
        }
        #endregion
    }
}