using DynamicData;
using DynamicData.Binding;
using Minimal_CS_Manga_Reader.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Minimal_CS_Manga_Reader
{
    public class BookmarkViewModel : ReactiveObject
    {
        private Action<BookmarkViewModel, Bookmark> _closeCallback;
        public ReactiveCommand<Unit, Unit> Open { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }
        public ReactiveCommand<Bookmark, Unit> DeleteBookmark { get; private set; }
        [Reactive] public Bookmark SelectedBookmark { get; set; }
        public IObservableCollection<Bookmark> BookmarkList { get; } = new ObservableCollectionExtended<Bookmark>();
        readonly IBookmarksSource BookmarksSource;
        public BookmarkViewModel(Action<BookmarkViewModel, Bookmark> closeCallback, IBookmarksSource bookmarks)
        {
            BookmarksSource = bookmarks ?? Locator.Current.GetService<IBookmarksSource>();

            BookmarksSource.Bookmarks.Connect().ObserveOn(RxApp.MainThreadScheduler)
                .Sort(new BookmarkComparer(StringComparison.OrdinalIgnoreCase))
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