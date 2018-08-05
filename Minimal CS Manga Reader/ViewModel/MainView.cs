using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Minimal_CS_Manga_Reader.Helper;
using Minimal_CS_Manga_Reader.Model;
using PropertyChanged;
using ReactiveUI;

namespace Minimal_CS_Manga_Reader.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public sealed class MainView
    {
        public ReactiveCommand<Unit, Unit> Submit { get; }

        #region Toolbar Stuff
        public int ImageMargin { get; set; } = 30; // 30 is placeholder, edit to settings later
        public string ImageMarginX { get; set; } // 30 is placeholder, edit to settings later
        public int ScrollIncrement { get; set; } = 100; // 100 is placeholder, edit to settings later
        public string ScrollIncrementX { get; set; }  // 100 is placeholder, edit to settings later
        public int ZoomScale { get; set; } = 100; // 100 is placeholder, edit to settings later
        public double ZoomScaleX { get; set; } = 1;
        public ReactiveCommand<Unit, int> IncreaseZoom { get; }
        public ReactiveCommand<Unit, int> DecreaseZoom { get; }

        // STUFF NOT CATEGORIZED
        //  public int ActiveImage => ImageList.Count;
        public string ActiveDirShow { get; set; }
        public int ImageCount { get; set; }

        public ReactiveList<string> ChaptersList => DataSource._chapterListShow;
        #endregion

        // Background
        public ReactiveList<string> BackgroundViewList { get; set; } = new ReactiveList<string> {"Black","White", "Silver"};
        public string ActiveBackgroundView { get; set; } = "Black";

        public ReactiveList<BitmapSource> ImageList => DataSource._imageList;

        public MainView()
        {
            DataSource.Initialize();
            ActiveDirShow = DataSource._activeDirShow;
            ScrollIncrementX = ScrollIncrement.ToString();
            #region Zoom
            IncreaseZoom = ReactiveCommand.Create(() => ZoomScale += 10);
            DecreaseZoom = ReactiveCommand.Create(() => ZoomScale >= 11 ? ZoomScale -= 10 : 10);
            this.WhenAnyValue(x => x.ZoomScale)
                .Where(ZoomScale => ZoomScale < 10)
                .Subscribe(x => ZoomScale = 10);
            this.WhenAnyValue(x => x.ZoomScale)
                .Subscribe(x => ZoomScaleX = ZoomScale == 100 ? 1 : ZoomScale / 99.999999999999);
            #endregion

            this.WhenAnyValue(x => x.ImageMargin)
                .Subscribe(x => ImageMarginX = $"0,0,0,{ImageMargin}");
            this.WhenAnyValue(x => x.ActiveDirShow)
                .Subscribe(x => {
                    DataSource._activeDir = DataSource._path + "\\" + ActiveDirShow;
                    UpdateAsync().ConfigureAwait(true); });
            this.WhenAnyValue(x => x.ImageList.Count)
                .Subscribe(x => ImageCount = ImageList.Count);
            this.WhenAnyValue(x => x.ScrollIncrement)
                .Subscribe(x => ScrollIncrementX = ScrollIncrement.ToString());
        }


        #region  Updater Task
        private Task T;
        private CancellationTokenSource ts { get; set; } = new CancellationTokenSource();

        public async Task UpdateAsync()
        {
            ts.Cancel();
            T?.Wait(ts.Token);
            ts = new CancellationTokenSource();
            DataSource.ClearImageList();
            ScrollHelper.Helper();
            T = await Task.Run(async () =>
            {
                await DataSource.DirUpdatedAsync(ts.Token).ConfigureAwait(true);
                return Task.CompletedTask;
            }).ConfigureAwait(true);
        }
        #endregion
    }
}
