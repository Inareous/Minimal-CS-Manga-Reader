using System;
using System.IO;

namespace Minimal_CS_Manga_Reader.Helper
{
    public class PathHelper
    {
        public static bool EnsureValidPath(string path)
        {
            return Directory.Exists(path) || (File.Exists(path) && EnsureAcceptedFileTypes(path));
        }

        public static bool EnsureAcceptedFileTypes(string filePath)
        {
            return filePath.EndsWith(".cbz", StringComparison.OrdinalIgnoreCase) ||
                filePath.EndsWith(".cbr", StringComparison.OrdinalIgnoreCase) ||
                filePath.EndsWith(".rar", StringComparison.OrdinalIgnoreCase) ||
                filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                filePath.EndsWith(".7z", StringComparison.OrdinalIgnoreCase) ||
                filePath.EndsWith(".tar", StringComparison.OrdinalIgnoreCase);
        }
    }
}
