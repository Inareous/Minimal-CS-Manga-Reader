using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace Minimal_CS_Manga_Reader
{
    public class SettingViewModel : ReactiveObject
    {
        private Action<SettingViewModel, bool> _closeCallback;

        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> SaveAndRefresh { get; }
        public ReactiveCommand<Unit, Unit> Close { get; }

        public IEnumerable<System.Drawing.Drawing2D.InterpolationMode> InterpolationMode
        {
            get
            {
                return Enum.GetValues(typeof(System.Drawing.Drawing2D.InterpolationMode))
                    .Cast<System.Drawing.Drawing2D.InterpolationMode>().Where(x => !x.Equals(System.Drawing.Drawing2D.InterpolationMode.Invalid));
            }
        }
        [Reactive] public System.Drawing.Drawing2D.InterpolationMode SelectedInterpolationMode { get; set; }

        public IEnumerable<System.Drawing.Drawing2D.SmoothingMode> SmoothingMode
        {
            get
            {
                return Enum.GetValues(typeof(System.Drawing.Drawing2D.SmoothingMode))
                    .Cast<System.Drawing.Drawing2D.SmoothingMode>().Where(x => !x.Equals(System.Drawing.Drawing2D.SmoothingMode.Invalid));
            }
        }
        [Reactive] public System.Drawing.Drawing2D.SmoothingMode SelectedSmoothingMode { get; set; }

        public IEnumerable<System.Drawing.Drawing2D.PixelOffsetMode> PixelOffsetMode
        {
            get
            {
                return Enum.GetValues(typeof(System.Drawing.Drawing2D.PixelOffsetMode))
                    .Cast<System.Drawing.Drawing2D.PixelOffsetMode>().Where(x => !x.Equals(System.Drawing.Drawing2D.PixelOffsetMode.Invalid));
            }
        }
        [Reactive] public System.Drawing.Drawing2D.PixelOffsetMode SelectedPixelOffsetMode { get; set; }

        public SettingViewModel(Action<SettingViewModel, bool> closeCallback)
        {
            _closeCallback = closeCallback;

            Save = ReactiveCommand.Create(() =>
            {
                Settings.Default.Save();
                _closeCallback(this, false);
            });

            SaveAndRefresh = ReactiveCommand.Create(() =>
            {
                Settings.Default.Save();
                _closeCallback(this, true);
            });

            Close = ReactiveCommand.Create(() => _closeCallback(this, false));
        }
    }
}