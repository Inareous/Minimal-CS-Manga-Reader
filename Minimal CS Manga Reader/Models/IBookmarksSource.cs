using DynamicData;
using System.Threading.Tasks;

namespace Minimal_CS_Manga_Reader.Models
{
    public interface IBookmarksSource
    {
        SourceList<Bookmark> Bookmarks { get; set; }

        bool Add(Bookmark bookmark);
        bool Delete(Bookmark bookmark);
        Task LoadAsync();
        Task SaveAsync();
    }
}