using DynamicData;
using DynamicData.Binding;
using Minimal_CS_Manga_Reader.Helper;
using Minimal_CS_Manga_Reader.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Minimal_CS_Manga_Reader
{
    public class BookmarkViewModel : ReactiveObject
    {
        private Action<BookmarkViewModel, Bookmark> _closeCallback;

        private IObservableCollection<Bookmark> _bookmarkList { get; } = new ObservableCollectionExtended<Bookmark>();
        public ReactiveCommand<Unit, Unit> Open { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }

        public ReactiveCommand<Bookmark, Unit> DeleteBookmark { get; private set; }
        [Reactive] public Bookmark SelectedBookmark { get; set; }
        
        public IObservableCollection<Bookmark> BookmarkList { get; } = new ObservableCollectionExtended<Bookmark>();
        public BookmarkViewModel(Action<BookmarkViewModel, Bookmark> closeCallback)
        {
            BookmarksSource.Bookmarks.Connect().ObserveOn(RxApp.MainThreadScheduler)
                .Bind(BookmarkList).DisposeMany().Subscribe();

            _closeCallback = closeCallback;
            Open = ReactiveCommand.Create(() =>
            {
                _closeCallback(this, SelectedBookmark);
            });

            Cancel = ReactiveCommand.Create(() =>
            {
                _closeCallback(this, null);
            });

            DeleteBookmark = ReactiveCommand.Create<Bookmark>(bookmark => BookmarksSource.Delete(bookmark));
        }

    }
}