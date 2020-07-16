using Minimal_CS_Manga_Reader.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Minimal_CS_Manga_Reader.Helper
{
    class BookmarkComparer : IComparer<Bookmark>
    {
        private readonly IComparer<string> _comparer;
        public BookmarkComparer(System.StringComparison stringComparison)
        {
            _comparer = new NaturalSortComparer(stringComparison);
        }
        public int Compare([AllowNull] Bookmark x, [AllowNull] Bookmark y)
        {
            return _comparer.Compare($"{x.ChapterPathTrimmed}_{x.ActiveChapterEntry.Name}", $"{y.ChapterPathTrimmed}_{y.ActiveChapterEntry.Name}");
        }
    }
}
