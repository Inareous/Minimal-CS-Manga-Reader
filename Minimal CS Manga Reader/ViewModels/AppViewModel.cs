using DynamicData;
using DynamicData.Binding;
using MaterialDesignThemes.Wpf;
using Minimal_CS_Manga_Reader.Helper;
using Minimal_CS_Manga_Reader.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
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
        public readonly IDataSource DataSource;
        public readonly IBookmarksSource BookmarksSource;
        public AppViewModel(IDataSource dataSource, IBookmarksSource bookmarksSource)
        {
            DataSource = dataSource ?? Locator.Current.GetService<IDataSource>();
            BookmarksSource = bookmarksSource ?? Locator.Current.GetService<IBookmarksSource>();

            InitializeWindow();

            #region Toggle Fullscreen

            ToggleFullscreen = ReactiveCommand.Create(() => IsFullscreen = !IsFullscreen);

            #endregion

            #region Scroll Change

            this.WhenAnyValue(x => x._activeImage).Subscribe(x => ActiveImage = x + 1);

            #endregion Scroll Change

            #region ChapterList Change

            DataSource.ChapterList.Connect().Bind(_chapterList).Subscribe();
            this.WhenAnyValue(x => x._chapterList.Count)
                .Subscribe(_ =>
                {
                    ChapterList = _chapterList?.Select(x => x.File).ToList();
                    ActiveIndex = ChapterList.Count > 0 && Settings.Default.OpenChapterOnLoadChoice == Enums.OpenChapterOnLoad.Last.ToString() ? ChapterList.Count-1 : 0;
                    if (ActiveIndex == 0) { UpdateAsync().ConfigureAwait(false); }
                    EnablePrevClick = ActiveIndex > 0;
                    EnableNextClick = ActiveIndex < ChapterList.Count - 1;
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
                    EnablePrevClick = ActiveIndex > 0;
                    EnableNextClick = ActiveIndex < ChapterList.Count - 1;
                });
            DataSource.ImageList.Connect().ObserveOn(RxApp.MainThreadScheduler)
                .OnItemAdded(x =>
                {
                    var sum = ImageDimension.Count == 0 ? 0 : ImageHeight.Last();
                    var width = Math.Min(x.Width, ViewportWidth);
                    var height = x.Width < ViewportWidth ? x.Height : x.Height * (ViewportWidth / x.Width);
                    sum += (height * ZoomScale) + _imageMarginSetter;
                    ImageDimension.Add((width, height));
                    ImageHeight.Add(sum);
                }).Bind(ImageList).DisposeMany().Subscribe();

            this.WhenAnyValue(x => x.ImageList.Count)
                .Subscribe(x =>
                {
                    ImageCount = x;
                });

            #endregion Chapter Change

            #region Viewport Change

            this.WhenAnyValue(x => x.ViewportWidth).Subscribe(newViewport =>
            {
                try
                {
                    for (int i = 0; i < ImageDimension.Count; i++)
                    {
                        if (Ts.IsCancellationRequested) return;
                        var width = Math.Min(ImageList[i].Width, newViewport);
                        var height = ImageList[i].Width < newViewport ? ImageList[i].Height : ImageList[i].Height * (newViewport / ImageList[i].Width);
                        ImageDimension[i] = (width, height);
                    }
                    UpdateImageHeight();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString()); // to do : fix here
                }
            });
            #endregion

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
                        UpdateImageHeight();
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
                        UpdateImageHeight();
                    }
                    ImageMarginSetter = _imageMarginSetter.ToString();
                    ImageMargin = $"0,0,0,{_imageMarginSetter}";
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

            OpenSetting = ReactiveCommand.CreateFromTask(ShowSettingDialog);
            OpenFolder = ReactiveCommand.CreateFromTask(OpenFolderDialog);
            OpenBookmark = ReactiveCommand.CreateFromTask(OpenBookmarkDialog);

            #endregion dialog
        }

        public bool AddBookmark()
        {
            return BookmarksSource.Add(new Bookmark(DataSource.Path, _chapterList[ActiveIndex]));
        }

        #region Method

        #region Scroll

        public void ScrollChanged()
        {
            if (ImageList.Count == 0 || _scrollHeight == 0) { _activeImage = 0; return; }

            while (_scrollHeight < ImageHeight.ElementAtOrDefault(_activeImage - 1) && _activeImage > 0) _activeImage--;

            while (_scrollHeight > ImageHeight.ElementAtOrDefault(_activeImage) && ImageHeight.ElementAtOrDefault(_activeImage) != default && _activeImage <= ImageHeight.Count) _activeImage++;

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

        private void ModifyTheme(Action<ITheme> modificationAction)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            modificationAction?.Invoke(theme);

            paletteHelper.SetTheme(theme);
        }

        private void UpdateImageHeight()
        {
            double sum = 0;
            for (int i = 0; i < ImageHeight.Count; i++)
            {
                var currHeight = ImageDimension[i].Item2 * ZoomScale;
                sum += currHeight + _imageMarginSetter;
                ImageHeight[i] = sum;
            }
        }

        #region Chapter Updater

        private Task T;

        private CancellationTokenSource Ts { get; set; } = new CancellationTokenSource();

        public async Task UpdateAsync()
        {
            if (ChapterList.Count > 0)
            {
                Ts.Cancel();
                T?.Wait(Ts.Token);
                Ts = new CancellationTokenSource();
                ImageHeight.Clear();
                ImageDimension.Clear();
                ScrollHelper.Helper();
                T = await Task.Run(async () =>
                {
                    await DataSource.PopulateImageAsync(_chapterList[ActiveIndex], Ts.Token).ConfigureAwait(false);
                    return T;
                }).ConfigureAwait(false);
            }
            else
            {
                DataSource.ImageList.Clear();
                ImageHeight.Clear();
                ImageDimension.Clear();
            }
        }

        #endregion Chapter Updater

        #endregion Method

        #region Property

        public IObservableCollection<BitmapSource> ImageList { get; } = new ObservableCollectionExtended<BitmapSource>();
        [Reactive] public List<string> ChapterList { get; set; } = new List<string>();
        public IObservableCollection<Entry> _chapterList { get; } = new ObservableCollectionExtended<Entry>();
        public List<double> ImageHeight { get; set; } = new List<double>();
        [Reactive] public List<ValueTuple<double, double>> ImageDimension { get; set; } = new List<ValueTuple<double, double>>();
        [Reactive] public string WindowTitle { get; set; } = "";
        [Reactive] public int ActiveIndex { get; set; } = 0;
        [Reactive] public bool IsDark { get; set; } = Settings.Default.IsDark;

        [Reactive] public bool IsScrollBarVisible { get; set; } = Settings.Default.IsScrollBarVisible;
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
        public ReactiveCommand<Unit, bool> ToggleFullscreen { get; }
        private int _scrollIncrement { get; set; } = Settings.Default.ScrollIncrement;
        private int _imageMarginSetter { get; set; } = Settings.Default.ImageMargin;
        private int _zoomScaleSetter { get; set; } = 100;

        [Reactive] public int ToolbarHeight { get; set; } = 30;

        [Reactive] public bool IsFullscreen { get; set; } = false;

        public double _scrollHeight = 0;
        [Reactive] public string ScrollIncrement { get; set; }
        [Reactive] public string ZoomScaleSetter { get; set; }
        [Reactive] public double ZoomScale { get; set; }
        [Reactive] public string ActiveBackgroundView { get; set; } = Settings.Default.Background;
        public Array BackgroundViewList { get; set; } = Enum.GetNames(typeof(Enums.ReaderBackground));

        #endregion Property

        #region Dialog stuff

        public Interaction<Unit, bool> SettingDialogInteraction { get; protected set; } = new Interaction<Unit, bool>();

        public Interaction<Unit, Bookmark> BookmarkDialogInteraction { get; protected set; } = new Interaction<Unit, Bookmark>();

        public Interaction<string, ValueTuple<Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult, string>> FolderDialogInteraction { get; protected set; } =
                new Interaction<string, ValueTuple<Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult, string>>();
        public ReactiveCommand<Unit, Unit> OpenSetting { get; }
        public ReactiveCommand<Unit, Unit> OpenBookmark { get; }
        public ReactiveCommand<Unit, Unit> OpenFolder { get; }
        [Reactive] public double ViewportWidth { get; set; }

        public async Task ShowSettingDialog()
        {
            try
            {
                var saveAndRefresh = await SettingDialogInteraction.Handle(Unit.Default);
                IsScrollBarVisible = Settings.Default.IsScrollBarVisible; //refresh

                if (saveAndRefresh) _ = UpdateAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.ToString());
            }
        }

        public async Task OpenFolderDialog()
        {
            try
            {
                var (callback, openChapterPath) = await FolderDialogInteraction.Handle(DataSource.Path);
                if (callback == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
                {
                    await DataSource.SetChapter(openChapterPath);
                    WindowTitle = $"{DataSource.Title}  -  Minimal CS Manga Reader";
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.ToString());
            }
        }

        public async Task OpenBookmarkDialog()
        {
            try
            {
                var callback = await BookmarkDialogInteraction.Handle(Unit.Default);
                if (callback == null) return; // Allow null but discard (for 'Cancel')
                if (callback.ChapterPath != DataSource.Path)
                {
                    await DataSource.SetChapter(callback.ChapterPath);
                    WindowTitle = $"{DataSource.Title}  -  Minimal CS Manga Reader";
                }
                if (callback.ActiveChapterEntry.AbsolutePath == DataSource.ActiveChapterPath) return;
                for (int i = 0; i < _chapterList.Count; i++)
                {
                    if (_chapterList[i].AbsolutePath == callback.ActiveChapterEntry.AbsolutePath)
                    {
                        ActiveIndex = i;
                        return;
                    }
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