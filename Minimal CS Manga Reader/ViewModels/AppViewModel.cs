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

            #endregion ChapterList Change

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
                    ImageHeightMod.Add(((x.Height + Sum) * ZoomScale) + (_imageMarginSetter * (HeightCount + 1)));
                    ImageCount = DataSource.ImageList.Count;
                }).Bind(ImageList).DisposeMany().Subscribe();

            #endregion Chapter Change

            #region Settings

            #region Zoom

            IncreaseZoom = ReactiveCommand.Create(() => ZoomScaleSetter = (_zoomScaleSetter + 10).ToString());
            DecreaseZoom = ReactiveCommand.Create(() => ZoomScaleSetter = _zoomScaleSetter >= 11 ? (_zoomScaleSetter - 10).ToString() : "10");
            this.WhenAnyValue(x => x.ZoomScaleSetter)
                .Subscribe(_ =>
                {
                    var succes = int.TryParse(ZoomScaleSetter, out int number);
                    if (succes && number != _zoomScaleSetter)
                    {
                        _zoomScaleSetter = number;
                        if (_zoomScaleSetter < 10) _zoomScaleSetter = 10;
                        ZoomScale = _zoomScaleSetter == 100 ? 1 : Math.Round(_zoomScaleSetter / 99.999999999999, 3);
                        Settings.Default.ZoomScale = _zoomScaleSetter;
                        Settings.Default.Save();
                        UpdateImageHeightMod();
                    }
                    ZoomScaleSetter = _zoomScaleSetter.ToString();
                });

            #endregion Zoom

            #region Margin

            this.WhenAnyValue(x => x.ImageMarginSetter)
                .Subscribe(_ =>
                {
                    var succes = int.TryParse(ImageMarginSetter, out int number);
                    if (succes && number != _imageMarginSetter)
                    {
                        _imageMarginSetter = number;
                        if (_imageMarginSetter < 0) _imageMarginSetter = 0;
                        Settings.Default.ImageMargin = _imageMarginSetter;
                        Settings.Default.Save();
                        UpdateImageHeightMod();
                    }
                    ImageMarginSetter = _imageMarginSetter.ToString();
                    ImageMargin = $"0,0,0,{_imageMarginSetter.ToString()}";
                });

            #endregion Margin

            #region Scroll Increment

            this.WhenAnyValue(x => x.ScrollIncrement)
                .Subscribe(_ =>
                {
                    var succes = int.TryParse(ScrollIncrement, out int number);
                    if (succes && number != _scrollIncrement)
                    {
                        _scrollIncrement = number;
                        Settings.Default.ScrollIncrement = _scrollIncrement;
                        Settings.Default.Save();
                    }
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

            #region dialog

            OpenSetting = ReactiveCommand.CreateFromTask(ShowSetting);

            #endregion dialog
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
            ZoomScale = _zoomScaleSetter == 100 ? 1 : Math.Round(_zoomScaleSetter / 99.999999999999, 3);
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
                ImageHeightMod[i] = (ImageHeight[i] * ZoomScale) + (_imageMarginSetter * (i + 1));
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
        private IObservableCollection<string> _chapterList { get; } = new ObservableCollectionExtended<string>();

        private List<double> ImageHeight { get; set; } = new List<double>();
        private List<double> ImageHeightMod { get; set; } = new List<double>();
        [Reactive] public string WindowTitle { get; set; } = "";
        [Reactive] public int ActiveIndex { get; set; } = 0;
        [Reactive] public bool IsDark { get; set; } = Settings.Default.IsDark;

        [Reactive] public bool EnablePrevClick { get; set; }

        [Reactive] public bool EnableNextClick { get; set; }
        [Reactive] private int _activeImage { get; set; } = 0;
        [Reactive] public int ActiveImage { get; set; } = 0;
        public ReactiveCommand<Unit, string> DecreaseZoom { get; }
        [Reactive] public int ImageCount { get; set; }
        [Reactive] public string ImageMarginSetter { get; set; }
        [Reactive] public string ImageMargin { get; set; }
        public ReactiveCommand<Unit, string> IncreaseZoom { get; }
        public ReactiveCommand<Unit, int> NextClick { get; }
        public ReactiveCommand<Unit, int> PreviousClick { get; }
        private int _scrollIncrement { get; set; } = Settings.Default.ScrollIncrement;
        private int _imageMarginSetter { get; set; } = Settings.Default.ImageMargin;
        private int _zoomScaleSetter { get; set; } = Settings.Default.ZoomScale;

        public double _scrollHeight = 0;
        [Reactive] public string ScrollIncrement { get; set; }
        [Reactive] public string ZoomScaleSetter { get; set; }
        [Reactive] public double ZoomScale { get; set; }
        [Reactive] public string ActiveBackgroundView { get; set; } = Settings.Default.Background;
        public List<string> BackgroundViewList { get; set; } = new List<string> { "Black", "White", "Silver" };

        #endregion Property

        #region Dialog stuff

        public Interaction<Unit, bool> SettingDialogInteraction { get; protected set; } = new Interaction<Unit, bool>();
        public ReactiveCommand<Unit, Unit> OpenSetting { get; }

        public async Task ShowSetting()
        {
            try
            {
                var saveAndRefresh = await this.SettingDialogInteraction.Handle(Unit.Default);

                if (saveAndRefresh)
                {
                    _ = UpdateAsync().ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.ToString());
            }
        }

        #endregion Dialog stuff
    }
}