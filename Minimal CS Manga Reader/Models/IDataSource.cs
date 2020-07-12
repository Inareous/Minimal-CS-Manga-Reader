using DynamicData;
using Minimal_CS_Manga_Reader.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Minimal_CS_Manga_Reader
{
    public interface IDataSource
    {
        string ActiveChapterPath { get; set; }
        SourceList<Entry> ChapterList { get; }
        SourceList<BitmapSource> ImageList { get; }
        string Path { get; }
        string Title { get; }

        Task InitializeAsync(string[] args);
        Task PopulateImageAsync(Entry entry, CancellationToken token);
        Task SetChapter(string path);
    }
}