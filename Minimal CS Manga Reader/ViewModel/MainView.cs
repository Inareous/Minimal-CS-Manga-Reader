using Minimal_CS_Manga_Reader.Helper;
using Minimal_CS_Manga_Reader.Model;
using Minimal_CS_Manga_Reader.Properties;
using PropertyChanged;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Minimal_CS_Manga_Reader.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public sealed class MainView
    {
        public MainView()
        {
            #region INIT

            DataSource.Initialize();
            WindowTitle = $"{DataSource._mangaTitle}  -  Minimal CS Manga Reader";
            ActiveIndex = ChaptersList.Count - 1;
            ImageMarginX = $"0,0,0,{ImageMargin}";
            ScrollIncrementX = ScrollIncrement.ToString();
            ZoomScaleX = ZoomScale == 100 ? 1 : ZoomScale / 99.999999999999;
            ActiveBackgroundView = Settings.Default.Background;

            #endregion INIT

            #region Zoom

            IncreaseZoom = ReactiveCommand.Create(() => ZoomScale += 10);
            DecreaseZoom = ReactiveCommand.Create(() => ZoomScale >= 11 ? ZoomScale -= 10 : 10);
            this.WhenAnyValue(x => x.ZoomScale)
                .Where(ZoomScale => ZoomScale < 10)
                .Subscribe(x => ZoomScale = 10);
            this.WhenAnyValue(x => x.ZoomScale)
                .Subscribe(x => ZoomScaleX = ZoomScale == 100 ? 1 : ZoomScale / 99.999999999999);

            #endregion Zoom

            #region Chapter Change

            NextClick = ReactiveCommand.Create(() => ActiveIndex = ActiveIndex >= ChaptersList.Count - 1 ? ChaptersList.Count - 1 : ActiveIndex + 1);
            PreviousClick = ReactiveCommand.Create(() => ActiveIndex = ActiveIndex <= 0 ? 0 : ActiveIndex - 1);
            this.WhenAnyValue(x => x.ActiveIndex)
                .Subscribe(x =>
                          {
                              ActiveDirShow = DataSource._chapterListShow[ActiveIndex];
                              UpdateAsync().ConfigureAwait(true);
                          });
            this.ImageList.ItemsAdded.Subscribe(z => {
                sum = ImageHeight.Count == 0 ? 0 : sum + ImageHeight[ImageHeight.Count - 1];
                ImageHeight.Add(z.Height+sum);
                ImageCount = ImageList.Count;
                ActiveImage = ImageHeight.Count == 0 ? 0 : ImageHeight[ImageHeight.Count - 1]; // Not Used yet
            });

            #endregion Chapter Change

            #region ActiveImage
            #endregion

            #region Settings Change

            this.WhenAnyValue(x => x.ImageMargin)
                .Subscribe(x => ImageMarginX = $"0,0,0,{ImageMargin}");
            this.WhenAnyValue(x => x.ScrollIncrement)
                .Subscribe(x => ScrollIncrementX = ScrollIncrement.ToString());

            #endregion Settings Change
        }

        public ReactiveList<BitmapSource> ImageList => DataSource._imageList;
        public ReactiveList<double> ImageHeight { get; set; } = new ReactiveList<double>();
        private double sum { get; set; }
        public string WindowTitle { get; set; }
        #region Toolbar Stuff

        // STUFF NOT CATEGORIZED
        //  public int ActiveImage => ImageList.Count;
        public string ActiveDirShow { get; set; }

        public int ActiveIndex { get; set; }
        public double ActiveImage { get; set; }
        public ReactiveList<string> ChaptersList => DataSource._chapterListShow;
        public ReactiveCommand<Unit, int> DecreaseZoom { get; }
        public int ImageCount { get; set; }
        public int ImageMargin { get; set; } = Settings.Default.ImageMargin;
        public string ImageMarginX { get; set; }
        public ReactiveCommand<Unit, int> IncreaseZoom { get; }
        public ReactiveCommand<Unit, int> NextClick { get; }
        public ReactiveCommand<Unit, int> PreviousClick { get; }
        public int ScrollIncrement { get; set; } = Settings.Default.ScrollIncrement;
        public string ScrollIncrementX { get; set; }
        public int ZoomScale { get; set; } = Settings.Default.ZoomScale;
        public double ZoomScaleX { get; set; }
        public string ActiveBackgroundView { get; set; } = Settings.Default.Background;
        public ReactiveList<string> BackgroundViewList { get; set; } = new ReactiveList<string> { "Black", "White", "Silver" };

        #endregion Toolbar Stuff

        #region Updater Task

        private Task T;
        private CancellationTokenSource Ts { get; set; } = new CancellationTokenSource();

        public async Task UpdateAsync()
        {
            DataSource._activeDir = DataSource._path + "\\" + ActiveDirShow;
            Ts.Cancel();
            T?.Wait(Ts.Token);
            Ts = new CancellationTokenSource();
            DataSource.ClearImageList();
            ScrollHelper.Helper();
            ImageHeight.Clear();
            T = await Task.Run(async () =>
            {
                await DataSource.DirUpdatedAsync(Ts.Token).ConfigureAwait(true);
                return Task.CompletedTask;
            }).ConfigureAwait(true);

        }

        #endregion Updater Task
    }
}