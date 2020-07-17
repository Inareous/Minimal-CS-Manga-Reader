using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Minimal_CS_Manga_Reader.Models
{
    public class Entry : IEquatable<Entry>
    {
        public Entry() {}
        public Entry(string absolutePath)
        {
            AbsolutePath = absolutePath;
            File = absolutePath.Split(Path.DirectorySeparatorChar)[^1];
            Name = Directory.Exists(AbsolutePath) ? File : Path.GetFileNameWithoutExtension(File);
        }

        public string AbsolutePath { get; set; }
        public string File { get; set; }
        public string Name { get; set; }

        public bool Equals([AllowNull] Entry other)
        {
            return AbsolutePath.Equals(other.AbsolutePath);
        }
    }
}
