using DynamicData;
using MaterialDesignThemes.Wpf;
using Minimal_CS_Manga_Reader.Helper;
using Minimal_CS_Manga_Reader.Model;
using Minimal_CS_Manga_Reader.Properties;
using PropertyChanged;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            ActiveIndex = ChaptersList.Count <= 0 ? 0 : ChaptersList.Count - 1;
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
                .Subscribe(x =>
                {
                    ZoomScaleX = ZoomScale == 100 ? 1 : ZoomScale / 99.999999999999;
                    Settings.Default.ZoomScale = ZoomScale;
                    Settings.Default.Save();
                    UpdateImageHeightMod();
                });

            #endregion Zoom

            #region Chapter Change

            NextClick = ReactiveCommand.Create(() => ActiveIndex = ActiveIndex >= ChaptersList.Count - 1 ? ChaptersList.Count - 1 : ActiveIndex + 1);
            PreviousClick = ReactiveCommand.Create(() => ActiveIndex = ActiveIndex <= 0 ? 0 : ActiveIndex - 1);
            this.WhenAnyValue(x => x.ActiveIndex)
                .Subscribe(x =>
                          {
                              ActiveDirShow = DataSource._chapterListShow.Count != 0 ? DataSource._chapterListShow[ActiveIndex] : "";
                              UpdateAsync().ConfigureAwait(true);
                              EnablePrevClick = ActiveIndex == 0 ? false : true;
                              EnableNextClick = ActiveIndex == ChaptersList.Count - 1 ? false : true;
                          });
            DataSource._imageList.Connect().OnItemAdded(x =>
            {
                sum = ImageHeight.Count == 0 ? 0 : ImageHeight[ImageHeight.Count - 1];
                ImageHeight.Add(x.Height + sum);
                ImageHeightMod.Add(x.Height + sum);
                if (ImageHeight.Count == ImageList.Count + 1)
                {
                    ImageHeight.RemoveAt(ImageHeight.Count - 1);
                    ImageHeightMod.RemoveAt(ImageHeightMod.Count - 1);
                }
                ImageCount = ImageList.Count;
            }).Bind(out _imageList).DisposeMany().Subscribe();

            #endregion Chapter Change

            #region Settings Change

            this.WhenAnyValue(x => x.ImageMargin)
                .Subscribe(x =>
                {
                    ImageMarginX = $"0,0,0,{ImageMargin}";
                    Settings.Default.ImageMargin = ImageMargin;
                    Settings.Default.Save();
                    UpdateImageHeightMod();
                });
            this.WhenAnyValue(x => x.ImageMargin)
                .Where(ImageMargin => ImageMargin < 0)
                .Subscribe(x => ImageMargin = 0);
            this.WhenAnyValue(x => x.ScrollIncrement)
                .Subscribe(x =>
                {
                    Settings.Default.ScrollIncrement = ScrollIncrement;
                    Settings.Default.Save();
                    ScrollIncrementX = ScrollIncrement.ToString();
                });
            this.WhenAnyValue(x => x.ActiveBackgroundView)
                .Subscribe(x =>
                {
                    Settings.Default.Background = ActiveBackgroundView;
                    Settings.Default.Save();
                });

            this.WhenAnyValue(x => x.isDark)
                .Subscribe(x =>
                {
                    Settings.Default.isDark = isDark;
                    Settings.Default.Save();
                    ModifyTheme(theme => theme.SetBaseTheme(isDark ? Theme.Dark : Theme.Light));
                });

            #endregion Settings Change

            this.WhenAnyValue(x => x._activeImage).Subscribe(x =>
            {
                ActiveImage = _activeImage + 1;
            });

        }

        #region Scroll

        public void ScrollChanged()
        {
            if (_scrollHeight >= _activeImageMin && _scrollHeight < _activeImageMax)
            {
                return;
            }

            if (_scrollHeight < _activeImageMin)
            {
                while (_scrollHeight < _activeImageMin)
                {
                    if (ImageHeightMod.ElementAtOrDefault(_activeImage - 1) != default)
                    {
                        _activeImageMin = ImageHeightMod[_activeImage - 1] + (ImageMargin * _activeImage);
                        _activeImage -= 1;
                    }
                    else { _activeImageMin = 0; _activeImage = 0; }
                }
                _activeImageMax = ImageHeightMod.ElementAtOrDefault(_activeImage) + (ImageMargin * _activeImage);

            }
            else if (_scrollHeight > _activeImageMax)

            {
                while (_scrollHeight > _activeImageMax)
                {
                    if (ImageHeightMod.ElementAtOrDefault(_activeImage + 1) != default)
                    {
                        _activeImageMax = ImageHeightMod[_activeImage + 1] + (ImageMargin * _activeImage);
                        _activeImage += 1;
                    } else { return; }
                }
                _activeImageMin = ImageHeightMod.ElementAtOrDefault(_activeImage - 1) + (ImageMargin * _activeImage);
            }

            if (_activeImage < 0)
            {
                _activeImage = 0;
            }
        }

        #endregion Scroll

        private double _activeImageMin { get; set; } = 0;
        private double _activeImageMax { get; set; } = 100;

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
                ImageHeightMod[i] = (ImageHeight[i] * ZoomScaleX) + ImageMargin;
            }
            _activeImageMax = ImageHeightMod.ElementAtOrDefault(1) != default ? ImageHeightMod[1] : 100;
        }


        private readonly ReadOnlyObservableCollection<BitmapSource> _imageList;
        public ReadOnlyObservableCollection<BitmapSource> ImageList => _imageList;

        public List<double> ImageHeight { get; set; } = new List<double>();
        public List<double> ImageHeightMod { get; set; } = new List<double>();
        private double sum { get; set; }
        public string WindowTitle { get; set; }

        #region Toolbar Stuff

        // STUFF NOT CATEGORIZED
        public string ActiveDirShow { get; set; }

        public int ActiveIndex { get; set; }
        public bool isDark { get; set; } = Settings.Default.isDark;

        public bool EnablePrevClick { get; set; }

        public bool EnableNextClick { get; set; }
        private int _activeImage { get; set; } = 0;
        public int ActiveImage { get; set; } = 0;
        public List<string> ChaptersList => DataSource._chapterListShow;
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
        public List<string> BackgroundViewList { get; set; } = new List<string> { "Black", "White", "Silver" };

        #endregion Toolbar Stuff

        #region Updater Task

        private Task T;
        public double _scrollHeight = 0;

        private CancellationTokenSource Ts { get; set; } = new CancellationTokenSource();

        public async Task UpdateAsync()
        {
            DataSource._activeDir = DataSource._path + "\\" + ActiveDirShow;
            Ts.Cancel();
            T?.Wait(Ts.Token);
            Ts = new CancellationTokenSource();
            ImageHeight.Clear();
            ImageHeightMod.Clear();
            DataSource.ClearImageList();
            ScrollHelper.Helper();
            T = await Task.Run(async () =>
            {
                await DataSource.DirUpdatedAsync(Ts.Token).ConfigureAwait(true);
                UpdateImageHeightMod();
                return Task.CompletedTask;
            }).ConfigureAwait(true);
            
        }

        #endregion Updater Task
    }
}