using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Minimal_CS_Manga_Reader.Models
{
    public class Entry : IEquatable<Entry>
    {
        public Entry() {}
        public Entry(string absolutePath) : this(absolutePath, true, false) { } //unused IsFolder

        public Entry(string absolutePath, bool NeedFileValidation, bool IsFolder)
        {
            AbsolutePath = absolutePath;
            File = absolutePath.Split(Path.DirectorySeparatorChar)[^1];
            if (NeedFileValidation) Name = Directory.Exists(AbsolutePath) ? File : Path.GetFileNameWithoutExtension(File);
            else if (IsFolder) Name = File;
            else Name = Path.GetFileNameWithoutExtension(File);
        }
        public Entry(string absolutePath, string file, string name)
        {
            AbsolutePath = absolutePath;
            File = file;
            Name = name;
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
