using DynamicData;
using Minimal_CS_Manga_Reader.Helper;
using Minimal_CS_Manga_Reader.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Minimal_CS_Manga_Reader
{
    public class SettingViewModel : ReactiveObject
    {
        private Action<SettingViewModel, bool> _closeCallback;
        private bool _initialContextIntegrated { get; set; }
        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> SaveAndRefresh { get; }
        public ReactiveCommand<Unit, Unit> Close { get; }
        [Reactive] public bool ContextIntegrated { get; set; }
        [Reactive] public bool FitImagesToScreen { get; set; }
        [Reactive] public Array OpenChapterOnLoadList { get; set; } = Enum.GetNames(typeof(Enums.OpenChapterOnLoad));
        [Reactive] public string OpenChapterOnLoad { get; set; }
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

        public SettingViewModel(Action<SettingViewModel, bool> closeCallback)
        {
            ContextIntegrated = RegistryContextManager.IsContextRegistry();
            _initialContextIntegrated = ContextIntegrated;
            FitImagesToScreen = Settings.Default.FitImagesToScreen;
            OpenChapterOnLoad = Settings.Default.OpenChapterOnLoadChoice;

            try
            {
                SelectedInterpolationMode = Settings.Default.InterpolationMode;
                SelectedSmoothingMode = Settings.Default.SmoothingMode;
                SelectedPixelOffsetMode = Settings.Default.PixelOffsetMode;
            }
            catch (Exception)
            {
                SelectedInterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                SelectedSmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                SelectedPixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
                Settings.Default.InterpolationMode = SelectedInterpolationMode;
                Settings.Default.SmoothingMode = SelectedSmoothingMode;
                Settings.Default.PixelOffsetMode = SelectedPixelOffsetMode;
                Settings.Default.Save();
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

            Close = ReactiveCommand.Create(() => _closeCallback(this, false));
        }

        private void SaveSettings(){
            Settings.Default.InterpolationMode = SelectedInterpolationMode;
            Settings.Default.SmoothingMode = SelectedSmoothingMode;
            Settings.Default.PixelOffsetMode = SelectedPixelOffsetMode;
            Settings.Default.FitImagesToScreen = FitImagesToScreen;
            Settings.Default.OpenChapterOnLoadChoice = OpenChapterOnLoad;
            Settings.Default.Save();
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