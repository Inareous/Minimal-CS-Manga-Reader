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
using System.Windows.Media;

namespace Minimal_CS_Manga_Reader
{
    public class SettingViewModel : ReactiveObject
    {
        private Action<SettingViewModel, bool> _closeCallback;
        private bool _initialContextIntegrated { get; set; }
        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> SaveAndRefresh { get; }
        public ReactiveCommand<Unit, Unit> Close { get; }
        public ReactiveCommand<Unit, Unit> SetAccentColor { get; }
        public ReactiveCommand<Unit, Unit> CancelAccentColor { get; }
        public ReactiveCommand<Unit, Unit> ResetAccentColor { get; }
        [Reactive] public bool ContextIntegrated { get; set; }
        [Reactive] public bool FitImagesToScreen { get; set; }
        [Reactive] public bool IsScrollBarVisible { get; set; }
        public IEnumerable<System.Drawing.Drawing2D.InterpolationMode> InterpolationMode
        {
            get
            {
                return Enum.GetValues(typeof(System.Drawing.Drawing2D.InterpolationMode))
                    .Cast<System.Drawing.Drawing2D.InterpolationMode>().Where(x => x != System.Drawing.Drawing2D.InterpolationMode.Invalid);
            }
        }

        [Reactive] public System.Drawing.Drawing2D.InterpolationMode SelectedInterpolationMode { get; set; }

        public IEnumerable<System.Drawing.Drawing2D.SmoothingMode> SmoothingMode
        {
            get
            {
                return Enum.GetValues(typeof(System.Drawing.Drawing2D.SmoothingMode))
                    .Cast<System.Drawing.Drawing2D.SmoothingMode>().Where(x => x != System.Drawing.Drawing2D.SmoothingMode.Invalid);
            }
        }

        [Reactive] public System.Drawing.Drawing2D.SmoothingMode SelectedSmoothingMode { get; set; } = System.Drawing.Drawing2D.SmoothingMode.Default;

        public IEnumerable<System.Drawing.Drawing2D.PixelOffsetMode> PixelOffsetMode
        {
            get
            {
                return Enum.GetValues(typeof(System.Drawing.Drawing2D.PixelOffsetMode))
                    .Cast<System.Drawing.Drawing2D.PixelOffsetMode>().Where(x => x != System.Drawing.Drawing2D.PixelOffsetMode.Invalid);
            }
        }

        [Reactive] public System.Drawing.Drawing2D.PixelOffsetMode SelectedPixelOffsetMode { get; set; } = System.Drawing.Drawing2D.PixelOffsetMode.Default;

        [Reactive] public string SelectedBackground { get; set; }
        public Array BackgroundViewList { get; set; } = Enum.GetNames(typeof(Enums.ReaderBackground));

        [Reactive] public Enums.Theme SelectedTheme { get; set; }
        public IEnumerable<Enums.Theme> ThemeList { get; set; } = Enum.GetValues(typeof(Enums.Theme)).Cast<Enums.Theme>();
        public IEnumerable<Enums.OpenChapterOnLoad> OpenChapterOnLoadList { get; set; } = Enum.GetValues(typeof(Enums.OpenChapterOnLoad)).Cast<Enums.OpenChapterOnLoad>();
        [Reactive] public Enums.OpenChapterOnLoad OpenChapterOnLoad { get; set; }
        public Color InitialAccentColor { get; set; } = ThemeEditor.theme.PrimaryMid.Color;
        [Reactive] public Color SelectedColor { get; set; } = ThemeEditor.theme.PrimaryMid.Color;
        [Reactive] public Brush SelectedBrush { get; set; }
        [Reactive] public bool IsPopupOpen { get; set; } = false;
         
        readonly IUserConfig Config;
        public SettingViewModel(Action<SettingViewModel, bool> closeCallback, IUserConfig config)
        {
            Config = config ?? Locator.Current.GetService<IUserConfig>();
            ContextIntegrated = RegistryContextManager.IsContextRegistry();
            _initialContextIntegrated = ContextIntegrated;
            FitImagesToScreen = Config.FitImagesToScreen;
            IsScrollBarVisible = Config.IsScrollBarVisible;
            OpenChapterOnLoad = Config.OpenChapterOnLoadChoice;
            SelectedBackground = Config.Background;
            SelectedTheme = Config.Theme;

            try
            {
                SelectedInterpolationMode = Config.InterpolationMode;
                SelectedSmoothingMode = Config.SmoothingMode;
                SelectedPixelOffsetMode = Config.PixelOffsetMode;
            }
            catch (Exception)
            {
                SelectedInterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                SelectedSmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                SelectedPixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
                Config.InterpolationMode = SelectedInterpolationMode;
                Config.SmoothingMode = SelectedSmoothingMode;
                Config.PixelOffsetMode = SelectedPixelOffsetMode;
                Config.Save();
            }
            _closeCallback = closeCallback;

            Save = ReactiveCommand.Create(() =>
            {
                SaveSettings();
                _closeCallback(this, false);
            });

            SaveAndRefresh = ReactiveCommand.Create(() =>
            {
                SaveSettings();
                _closeCallback(this, true);
            });

            Close = ReactiveCommand.Create(() => 
            {
                _closeCallback(this, false);
                if (SelectedTheme != Config.Theme)
                {
                    // Restore theme
                    ThemeEditor.ModifyTheme(Config.Theme);
                }
                ThemeEditor.ChangePrimaryColor(Config.AccentColor);
            });

            SetAccentColor = ReactiveCommand.Create(() =>
            {
                InitialAccentColor = SelectedColor;
                ThemeEditor.ChangePrimaryColor(SelectedColor);
                IsPopupOpen = false;
            });

            CancelAccentColor = ReactiveCommand.Create(() =>
            {
                SelectedColor = Config.AccentColor;
                IsPopupOpen = false;
            });

            ResetAccentColor = ReactiveCommand.Create(() =>
            {
                InitialAccentColor = Color.FromArgb(255, 154, 103, 234);
                SelectedColor = InitialAccentColor;
                ThemeEditor.ChangePrimaryColor(InitialAccentColor);
                IsPopupOpen = false;
            });

            this.WhenValueChanged(x => x.SelectedTheme).Subscribe(x => 
            {
                ThemeEditor.ModifyTheme(x);
            });

            this.WhenAnyValue(x => x.SelectedColor).Subscribe(x =>
            {
                SelectedBrush = new SolidColorBrush(SelectedColor);
            });
        }

        private void SaveSettings(){
            Config.InterpolationMode = SelectedInterpolationMode;
            Config.SmoothingMode = SelectedSmoothingMode;
            Config.PixelOffsetMode = SelectedPixelOffsetMode;
            Config.FitImagesToScreen = FitImagesToScreen;
            Config.IsScrollBarVisible = IsScrollBarVisible;
            Config.OpenChapterOnLoadChoice = OpenChapterOnLoad;
            Config.Background = SelectedBackground;
            Config.Theme = SelectedTheme;
            Config.AccentColor = SelectedColor;
            Config.Save();
            if (_initialContextIntegrated != ContextIntegrated)
            {
                try
                {
                    RegistryContextManager.ChangeContextIntegrated(ContextIntegrated);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                }
            }
        }
    }
}