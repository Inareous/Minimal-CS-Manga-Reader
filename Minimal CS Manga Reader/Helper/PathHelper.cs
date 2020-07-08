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

        public static bool EnsureAcceptedFileTypes(string file)
        {
            return file.EndsWith(".cbz", StringComparison.OrdinalIgnoreCase)
                || file.EndsWith(".cbr", StringComparison.OrdinalIgnoreCase)
                || file.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)
                || file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                || file.EndsWith(".7z", StringComparison.OrdinalIgnoreCase)
                || file.EndsWith(".tar", StringComparison.OrdinalIgnoreCase);
        }

        public static bool EnsureAcceptedImageTypes(string file)
        {
            return file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                || file.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase);
        }

        public static bool EnsureValidArchives(string filePath)
        {
            return ZipArchive.IsZipFile(filePath)
                || RarArchive.IsRarFile(filePath)
                || SevenZipArchive.IsSevenZipFile(filePath)
                || TarArchive.IsTarFile(filePath);
        }
    }
}
