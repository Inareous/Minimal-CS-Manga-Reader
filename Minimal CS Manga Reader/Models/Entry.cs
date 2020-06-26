using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Minimal_CS_Manga_Reader.Models
{
    public class Entry
    {
        public Entry(string absolutePath)
        {
            AbsolutePath = absolutePath;
            Name = absolutePath.Split('\\')[^1];
        }

        public string AbsolutePath { get; set; }
        public string Name { get; set; }

    }
}
