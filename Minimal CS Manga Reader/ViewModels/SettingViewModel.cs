using ReactiveUI;
using System;
using System.Reactive;

namespace Minimal_CS_Manga_Reader
{
    public class SettingViewModel : ReactiveObject
    {
        private Action<SettingViewModel, bool> _closeCallback;

        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> Close { get; }

        public SettingViewModel(Action<SettingViewModel, bool> closeCallback)
        {
            _closeCallback = closeCallback;

            Save = ReactiveCommand.Create(() =>
            {
                Settings.Default.Save();
                _closeCallback(this, true);
            });

            Close = ReactiveCommand.Create(() => _closeCallback(this, false));
        }
    }
}