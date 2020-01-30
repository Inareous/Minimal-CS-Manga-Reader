using DynamicData;
using DynamicData.Binding;
using MaterialDesignThemes.Wpf;
using Minimal_CS_Manga_Reader.Helper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Minimal_CS_Manga_Reader
{
    public class AppViewModel : ReactiveObject
    {
        public AppViewModel()
        {
            InitializeWindow();

            #region Scroll Change

            this.WhenAnyValue(x => x._activeImage).Subscribe(x => ActiveImage = x + 1);

            #endregion Scroll Change

            #region ChapterList Change
            DataSource.ChapterList.Connect().ObserveOn(RxApp.MainThreadScheduler)
                .Bind(_chapterList).Subscribe();
            this.WhenAnyValue(x => x._chapterList.Count)
                .Subscribe(_ =>
                {
                    CreateChapterListTrimmed();
                    if (ChapterList.Count > 0)
                    {
                        ActiveIndex = ChapterList.Count - 1;
                        if (ActiveIndex == 0) { UpdateAsync().ConfigureAwait(false); }
                    }
                    EnablePrevClick = ActiveIndex != 0;
                    EnableNextClick = ActiveIndex != ChapterList.Count - 1;
                });
            #endregion
            #region Chapter Change

            NextClick = ReactiveCommand.Create(() => ActiveIndex = ActiveIndex >= ChapterList.Count - 1 ? ChapterList.Count - 1 : ActiveIndex + 1);
            PreviousClick = ReactiveCommand.Create(() => ActiveIndex = ActiveIndex <= 0 ? 0 : ActiveIndex - 1);
            this.WhenAnyValue(x => x.ActiveIndex)
                .Subscribe(_ =>
                {
                    if (ChapterList.Count > 0)
                    {
                        UpdateAsync().ConfigureAwait(false);
                    }
                    EnablePrevClick = ActiveIndex != 0;
                    EnableNextClick = ActiveIndex != ChapterList.Count - 1;
                });
            DataSource.ImageList.Connect().ObserveOn(RxApp.MainThreadScheduler)
                .OnItemAdded(x =>
                {
                    var HeightCount = ImageHeight.Count;
                    var Sum = HeightCount == 0 ? 0 : ImageHeight[^1];
                    ImageHeight.Add(x.Height + Sum);
                    ImageHeightMod.Add(((x.Height + Sum) * ZoomScale) + (ImageMarginSetter * (HeightCount + 1)));
                    ImageCount = DataSource.ImageList.Count;
                }).Bind(ImageList).DisposeMany().Subscribe();
            
            #endregion Chapter Change

            #region Settings

            #region Zoom

            IncreaseZoom = ReactiveCommand.Create(() => ZoomScaleSetter += 10);
            DecreaseZoom = ReactiveCommand.Create(() => ZoomScaleSetter >= 11 ? ZoomScaleSetter -= 10 : 10);
            this.WhenAnyValue(x => x.ZoomScaleSetter)
                .Subscribe(_ =>
                {
                    if (ZoomScaleSetter < 10) ZoomScaleSetter = 10;
                    ZoomScale = ZoomScaleSetter == 100 ? 1 : ZoomScaleSetter / 99.999999999999;
                    Settings.Default.ZoomScale = ZoomScaleSetter;
                    Settings.Default.Save();
                    UpdateImageHeightMod();
                });

            #endregion Zoom

            #region Margin

            this.WhenAnyValue(x => x.ImageMarginSetter)
                .Subscribe(_ =>
                {
                    if (ImageMarginSetter < 0) ImageMarginSetter = 0;
                    ImageMargin = $"0,0,0,{ImageMarginSetter}";
                    Settings.Default.ImageMargin = ImageMarginSetter;
                    Settings.Default.Save();
                    UpdateImageHeightMod();
                });

            #endregion Margin

            #region Scroll Increment

            this.WhenAnyValue(x => x.ScrollIncrement)
                .Subscribe(_ =>
                {
                    Settings.Default.ScrollIncrement = _scrollIncrement;
                    Settings.Default.Save();
                    ScrollIncrement = ScrollIncrement.ToString();
                    ScrollIncrement = _scrollIncrement.ToString();
                });

            #endregion Scroll Increment

            #region Background

            this.WhenAnyValue(x => x.ActiveBackgroundView)
                .Subscribe(_ =>
                {
                    Settings.Default.Background = ActiveBackgroundView;
                    Settings.Default.Save();
                });

            #endregion Background

            #region Dark Mode

            this.WhenAnyValue(x => x.IsDark)
                .Subscribe(_ =>
                {
                    Settings.Default.IsDark = IsDark;
                    Settings.Default.Save();
                    ModifyTheme(theme => theme.SetBaseTheme(IsDark ? Theme.Dark : Theme.Light));
                });

            #endregion Dark Mode

            #endregion Settings
        }

        #region Method

        #region Scroll

        public void ScrollChanged()
        {
            if (ImageList.Count == 0 || _scrollHeight == 0) { _activeImage = 0; return; }

            while (_scrollHeight < ImageHeightMod.ElementAtOrDefault(_activeImage - 1) && _activeImage > 0) _activeImage--;

            while (_scrollHeight > ImageHeightMod.ElementAtOrDefault(_activeImage) && ImageHeightMod.ElementAtOrDefault(_activeImage) != default && _activeImage <= ImageHeightMod.Count) _activeImage++;

            if (_activeImage < 0) _activeImage = 0;
        }

        #endregion Scroll

        private void InitializeWindow()
        {
            WindowTitle = $"{DataSource.Title}  -  Minimal CS Manga Reader";
            ImageMargin = $"0,0,0,{ImageMarginSetter}";
            ScrollIncrement = _scrollIncrement.ToString();
            ZoomScale = ZoomScaleSetter == 100 ? 1 : ZoomScaleSetter / 99.999999999999;
            ActiveBackgroundView = Settings.Default.Background;
        }

        private void CreateChapterListTrimmed()
        {
            ChapterList = _chapterList?.Select(x => x.Split('\\')[^1]).ToList();
        }

        private void ModifyTheme(Action<ITheme> modificationAction)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            modificationAction?.Invoke(theme);

            paletteHelper.SetTheme(theme);
        }

        private void UpdateImageHeightMod()
        {
            for (int i = 0; i < ImageHeightMod.Count; ++i)
            {
                ImageHeightMod[i] = (ImageHeight[i] * ZoomScale) + (ImageMarginSetter * (i + 1));
            }
        }

        #region Chapter Updater

        private Task T;

        private CancellationTokenSource Ts { get; set; } = new CancellationTokenSource();

        public async Task UpdateAsync()
        {
            DataSource.ActiveChapterPath = ChapterList.Count != 0 ? _chapterList[ActiveIndex] : "\\";
            Ts.Cancel();
            T?.Wait(Ts.Token);
            Ts = new CancellationTokenSource();
            ImageHeight.Clear();
            ImageHeightMod.Clear();
            ScrollHelper.Helper();
            T = await Task.Run(async () =>
            {
                await DataSource.PopulateImageAsync(Ts.Token).ConfigureAwait(false);
                return T;
            }).ConfigureAwait(false);
        }

        #endregion Chapter Updater

        #endregion Method

        #region Property

        public IObservableCollection<BitmapSource> ImageList { get; } = new ObservableCollectionExtended<BitmapSource>();
        [Reactive] public List<string> ChapterList { get; set; } = new List<string>();
        private IObservableCollection<string> _chapterList { get;} = new ObservableCollectionExtended<string>();

        private List<double> ImageHeight { get; set; } = new List<double>();
        private List<double> ImageHeightMod { get; set; } = new List<double>();
        [Reactive] public string WindowTitle { get; set; } = "";
        [Reactive] public int ActiveIndex { get; set; } = 0;
        [Reactive] public bool IsDark { get; set; } = Settings.Default.IsDark;

        [Reactive] public bool EnablePrevClick { get; set; }

        [Reactive] public bool EnableNextClick { get; set; }
        [Reactive] private int _activeImage { get; set; } = 0;
        [Reactive] public int ActiveImage { get; set; } = 0;
        public ReactiveCommand<Unit, int> DecreaseZoom { get; }
        [Reactive] public int ImageCount { get; set; }
        [Reactive] public int ImageMarginSetter { get; set; } = Settings.Default.ImageMargin;
        [Reactive] public string ImageMargin { get; set; }
        public ReactiveCommand<Unit, int> IncreaseZoom { get; }
        public ReactiveCommand<Unit, int> NextClick { get; }
        public ReactiveCommand<Unit, int> PreviousClick { get; }
        public int _scrollIncrement { get; set; } = Settings.Default.ScrollIncrement;
        public double _scrollHeight = 0;
        [Reactive] public string ScrollIncrement { get; set; }
        [Reactive] public int ZoomScaleSetter { get; set; } = Settings.Default.ZoomScale;
        [Reactive] public double ZoomScale { get; set; }
        [Reactive] public string ActiveBackgroundView { get; set; } = Settings.Default.Background;
        public List<string> BackgroundViewList { get; set; } = new List<string> { "Black", "White", "Silver" };

        #endregion Property
    }
}