using DynamicData;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Minimal_CS_Manga_Reader.Models
{
    public interface IDataSource
    {
        string ActiveChapterPath { get; set; }
        SourceList<Entry> ChapterList { get; }
        SourceList<BitmapSource> ImageList { get; }
        string Path { get; set; }
        string Title { get; set; }

        void Initialize(string[] args);
        Task PopulateImageAsync(Entry entry, CancellationToken token);
        Task<bool> SetChapter(string path);
    }
}