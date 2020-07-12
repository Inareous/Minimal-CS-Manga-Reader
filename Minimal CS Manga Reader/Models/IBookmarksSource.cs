using DynamicData;
using System.Threading.Tasks;

namespace Minimal_CS_Manga_Reader.Models
{
    public interface IBookmarksSource
    {
        SourceList<Bookmark> Bookmarks { get; set; }

        void Add(Bookmark bookmark);
        void Delete(Bookmark bookmark);
        Task LoadAsync();
        Task SaveAsync();
    }
}