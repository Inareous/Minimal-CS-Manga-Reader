using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Disposables;
using System.Windows.Controls;

namespace Minimal_CS_Manga_Reader
{
    public partial class BookmarkView : UserControl, IViewFor<BookmarkViewModel>
    {
        public BookmarkView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.Cancel, view => view.Cancel).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Open, view => view.Open).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.BookmarkList, view=> view.Bookmarks.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedBookmark, view => view.Bookmarks.SelectedItem).DisposeWith(d);
            });

        }

        [Reactive] public BookmarkViewModel ViewModel { get; set; }
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (BookmarkViewModel)value;
        }
    }
}