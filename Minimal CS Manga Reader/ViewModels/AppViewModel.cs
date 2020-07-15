using DynamicData;
using DynamicData.Binding;
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
        public readonly IUserConfig Config;
        public AppViewModel(IDataSource dataSource, IBookmarksSource bookmarksSource, IUserConfig config)
        {
            DataSource = dataSource ?? Locator.Current.GetService<IDataSource>();
            BookmarksSource = bookmarksSource ?? Locator.Current.GetService<IBookmarksSource>();
            Config = config ?? Locator.Current.GetService<IUserConfig>();

            Initialize();

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
                });

            #endregion ChapterList Change

            #region Chapter Change

            NextClick = ReactiveCommand.Create(() => _activeIndex = _activeIndex >= ChapterList.Count - 1 ? ChapterList.Count - 1 : _activeIndex + 1);
            PreviousClick = ReactiveCommand.Create(() => _activeIndex = _activeIndex <= 0 ? 0 : _activeIndex - 1);
            this.WhenAnyValue(x => x._activeIndex)
                .Subscribe(activeIndex =>
                {
                    ActiveIndex = activeIndex;
                    EnablePrevClick = activeIndex > 0;
                    EnableNextClick = activeIndex < ChapterList.Count - 1;
                });
            this.WhenAnyValue(x => x.ActiveIndex)
                .Subscribe(_ =>
                {
                    UpdateAsync().ConfigureAwait(false);
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
                        Config.ImageMargin = _imageMarginSetter;
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
                        Config.ScrollIncrement = _scrollIncrement;
                    }
                    ScrollIncrement = _scrollIncrement.ToString();
                });

            #endregion Scroll Increment

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

        private void Initialize()
        {
            _ = Task.Run(async () =>
            {
                DataSource.Initialize(Environment.GetCommandLineArgs());
                var updated = await DataSource.SetChapter(DataSource.Path); // Self-initialize
                WindowTitle = $"{DataSource.Title}  -  Minimal CS Manga Reader";
                if (updated)
                {
                    // Refresh from source instead of ViewModel since it's not initialized yet
                    _activeIndex = DataSource.ChapterList.Count > 0 && Config.OpenChapterOnLoadChoice == Enums.OpenChapterOnLoad.Last ? DataSource.ChapterList.Count - 1 : 0;
                    ActiveIndex = _activeIndex;
                }
            });
            ThemeEditor.ModifyTheme(Config.Theme);
            ImageMargin = $"0,0,0,{ImageMarginSetter}";
            ZoomScale = _zoomScaleSetter == 100 ? 1 : Math.Round(_zoomScaleSetter / 99.999999999999, 3);
            ActiveBackgroundView = Config.Background;
            IsScrollBarVisible = Config.IsScrollBarVisible;
            _scrollIncrement = Config.ScrollIncrement;
            ScrollIncrement = _scrollIncrement.ToString();
            _imageMarginSetter = Config.ImageMargin;
            ActiveBackgroundView = Config.Background;
        }

        private void RefreshActiveIndex()
        {
            _activeIndex = _chapterList.Count > 0 && Config.OpenChapterOnLoadChoice == Enums.OpenChapterOnLoad.Last ? _chapterList.Count - 1 : 0;
            ActiveIndex = _activeIndex;
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
        [Reactive] private int _activeIndex { get; set; } = 0;
        [Reactive] public bool IsScrollBarVisible { get; set; }
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
        private int _scrollIncrement { get; set; }
        private int _imageMarginSetter { get; set; }
        private int _zoomScaleSetter { get; set; } = 100;
        [Reactive] public int ToolbarHeight { get; set; } = 30;
        [Reactive] public bool IsFullscreen { get; set; } = false;
        public double _scrollHeight = 0;
        [Reactive] public string ScrollIncrement { get; set; }
        [Reactive] public string ZoomScaleSetter { get; set; }
        [Reactive] public double ZoomScale { get; set; }
        [Reactive] public string ActiveBackgroundView { get; set; } 

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
                //refresh
                IsScrollBarVisible = Config.IsScrollBarVisible;
                ActiveBackgroundView = Config.Background;

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
                    var updated = await DataSource.SetChapter(openChapterPath);
                    if (updated) RefreshActiveIndex();
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
                    var updated = await DataSource.SetChapter(callback.ChapterPath);
                    
                    WindowTitle = $"{DataSource.Title}  -  Minimal CS Manga Reader";
                }
                if (callback.ActiveChapterEntry.AbsolutePath == DataSource.ActiveChapterPath) return;
                for (int i = 0; i < _chapterList.Count; i++)
                {
                    if (_chapterList[i].AbsolutePath == callback.ActiveChapterEntry.AbsolutePath)
                    {
                        _activeIndex = i;
                        return;
                    }
                }
                // No matching chapter
                RefreshActiveIndex();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.ToString());
            }
        }
        #endregion Dialog stuff
    }
}