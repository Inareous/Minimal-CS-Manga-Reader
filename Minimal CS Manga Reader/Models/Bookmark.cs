using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Minimal_CS_Manga_Reader.Models
{
    public class Bookmark : IEquatable<Bookmark>
    {
        public Bookmark() { }
        public Bookmark(string chapterPath, Entry activeChapterEntry)
        {
            ChapterPath = chapterPath;
            ChapterPathTrimmed = ChapterPath.Split(Path.DirectorySeparatorChar)[^1];
            ActiveChapterEntry = activeChapterEntry;
        }

        public string ChapterPath { get; set; }
        public string ChapterPathTrimmed { get; set; }
        public Entry ActiveChapterEntry { get; set; }

        public bool Equals([AllowNull] Bookmark other)
        {
            if (other == null)
                return false;

            return ChapterPath.Equals(other.ChapterPath) && ActiveChapterEntry.Equals(other.ActiveChapterEntry);
        }
    }
}
