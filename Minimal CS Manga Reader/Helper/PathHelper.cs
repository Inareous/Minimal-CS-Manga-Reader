using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.Zip;
using System.IO;
using System;
using Minimal_CS_Manga_Reader.Models;

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

        public static Enums.ImageType EnsureAcceptedImageTypes(string file)
        {
            if (file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                || file.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) return Enums.ImageType.Default;
            if (file.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)) return Enums.ImageType.WebP;
            return Enums.ImageType.NotImage;
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
