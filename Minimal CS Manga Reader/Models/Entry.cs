using System.IO;

namespace Minimal_CS_Manga_Reader.Models
{
    public class Entry
    {
        public Entry(string absolutePath)
        {
            AbsolutePath = absolutePath;
            File = absolutePath.Split(Path.DirectorySeparatorChar)[^1];
            Name = Path.GetFileNameWithoutExtension(File);
        }

        public string AbsolutePath { get; set; }
        public string File { get; set; }
        public string Name { get; set; }

    }
}
