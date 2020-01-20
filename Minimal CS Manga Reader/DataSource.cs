using DynamicData;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Minimal_CS_Manga_Reader.Model
{
    public static class DataSource
    {
        private static readonly string[] _args = Environment.GetCommandLineArgs();

        public static string
            _path = Settings.Default
                .Path; //Always assuming path exist since we will use context registry, no need for sanity check

        public static List<string> _chapterListShow { get; set; } = new List<string>();

        public static string _activeDir { get; set; }

        public static SourceList<BitmapSource> _imageList { get; set; } = new SourceList<BitmapSource>();

        public static List<string> _chapterList { get; set; }

        public static string _imageCountShow { get; set; }

        public static string Title { get; set; }

        public static void Initialize()
        {
            bool notZip = true;
            if (_path.Equals("FirstTimeNotSet") || _path.Equals(null)) _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (_args.Length >= 2)
            {
                foreach (var args in _args)
                {
                    if (!args.Contains("-path=")) continue;
                    var x = args.Replace("-path=", "");
                    if (Directory.Exists(x))
                    {
                        notZip = true;
                        _path = x;
                        Settings.Default.Path = _path;
                    }
                    else if (ZipArchive.IsZipFile(x) || RarArchive.IsRarFile(x))
                    {
                        notZip = false;
                        _path = x;
                        Settings.Default.Path = _path;
                    }
                }
                Settings.Default.Save();
            }
            //
            Title = _path.Split('\\').ToArray()[^1];
            //
            if (notZip) { _chapterList = DataHandler.FetchChapters(_path); } else { _chapterList.Add(_path); }
            if (_chapterList.Count.Equals(0)) return;
            _chapterListShow = SetChapters();
            string _activeDirShow = _chapterListShow.Count == 0 ? "" : _chapterListShow[^1];
            _activeDir = notZip ? _path + "\\" + _activeDirShow : _path.Replace("\\" + _activeDirShow, "");
        }

        public static List<string> SetChapters()
        {
            List<string> chapters = new List<string>();
            foreach (var chapter in _chapterList)
            {
                var x = chapter.Split('\\').ToList();
                chapters.Add(x[^1]);
            }
            return chapters;
        }

        public static Task DirUpdatedAsync(CancellationToken token)
        {
            try
            {
                var T = Task.Run(() => DataHandler.FetchImages(_activeDir, _imageList, token), token);
                token.ThrowIfCancellationRequested();
                return T;
            }
            catch (OperationCanceledException)
            {
                ClearImageList();
                return Task.CompletedTask;
            }
        }

        public static void ClearImageList()
        {
            _imageList.Clear();
        }
    }
}