using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
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
                this.Bind(ViewModel, vm => vm.SelectedBookmark, view => view.Bookmarks.SelectedItem).DisposeWith(d);
            });

            Bookmarks.Events().MouseDoubleClick.Subscribe(x =>
            {

                System.Windows.DependencyObject obj = (System.Windows.DependencyObject)x.OriginalSource;

                while (obj != null && obj != Bookmarks)
                {
                    if (obj.GetType() == typeof(Button)) return; //Exclude delete button
                    if (obj.GetType() == typeof(ListBoxItem))
                    {
                        ViewModel.Open.Execute().Subscribe();
                    }
                    obj = System.Windows.Media.VisualTreeHelper.GetParent(obj);
                }
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