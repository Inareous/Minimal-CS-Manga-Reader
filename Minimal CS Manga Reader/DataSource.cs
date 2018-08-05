using Minimal_CS_Manga_Reader.Properties;
using ReactiveUI;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
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


        public static ReactiveList<string> _chapterList { get; set; }

        public static ReactiveList<string> _chapterListShow { get; set; }

        public static string _activeDirShow { get; set; }

        public static string _activeDir { get; set; }

        public static ReactiveList<BitmapSource> _imageList { get; set; } = new ReactiveList<BitmapSource>();

        public static int _activeImage { get; set; } = 1;
        public static bool notZip = true;


        public static string _imageCountShow { get; set; }

        public static void Initialize()
        {
            if (_path.Equals("FirstTimeOpenNotSet"))
                _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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
            if (notZip) {_chapterList = DataHandler.FetchChapters(_path);} else { _chapterList.Add(_path);}
            if (_chapterList.Count.Equals(0)) return;
            _chapterListShow = SetChapters();
            _activeDirShow = _chapterListShow.Count == 0 ? "" : _chapterListShow[_chapterListShow.Count-1];
            _activeDir = notZip ? _path + "\\" + _activeDirShow : _path.Replace("\\" + _activeDirShow, "");
        }


        public static ReactiveList<string> SetChapters()
        {
            ReactiveList<string> c = new ReactiveList<string>();
            foreach (var chapter in _chapterList)
            {
                var x = chapter.Split('\\').ToList();
                var y = x[x.Count - 1];
                c.Add(y);
            }
            return c;
        }


        public static Task DirUpdatedAsync(CancellationToken token)
        {
            try
            {
                Task.Run(() => DataHandler.FetchImages(_activeDir, _imageList, token), token);
                token.ThrowIfCancellationRequested();
                return Task.CompletedTask;
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