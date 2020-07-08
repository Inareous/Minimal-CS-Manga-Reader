using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using SharpCompress.Archives.Tar;
using System.IO;
using System;
using SharpCompress.Archives.SevenZip;

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

        public static bool EnsureValidArchives(string filePath)
        {
            return ZipArchive.IsZipFile(filePath) || RarArchive.IsRarFile(filePath)
                || SevenZipArchive.IsSevenZipFile(filePath) || TarArchive.IsTarFile(filePath);
        }
    }
}
