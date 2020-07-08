using DynamicData;
using Minimal_CS_Manga_Reader.Helper;
using Minimal_CS_Manga_Reader.Models;
using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using PixelFormat = System.Windows.Media.PixelFormat;

namespace Minimal_CS_Manga_Reader
{
    public class DataCollector
    {
        public async Task<IEnumerable<Entry>> GetChapterListAsync(string Path, bool IsArchive)
        {
            if (IsArchive) return new List<Entry> { new Entry(Path) };
            var FilesInFolder = Task.Run(() => Directory.EnumerateFiles(Path, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".jpg") || s.EndsWith(".png")));
            var Files = Task.Run(() => Directory.EnumerateFiles(Path, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(s => PathHelper.EnsureAcceptedFileTypes(s)));
            var Folders = Task.Run(() => Directory.EnumerateDirectories(Path, "*", SearchOption.TopDirectoryOnly));
            await Task.WhenAll(Files, Folders, FilesInFolder).ConfigureAwait(false);

            var ReturnList = (Files.Result ?? Enumerable.Empty<string>()).Concat(Folders.Result ?? Enumerable.Empty<string>());
            var Entries = ReturnList.Select(x => new Entry(x));
            var OrderedEntries = Entries.OrderBy(x => x.Name, new NaturalSortComparer(StringComparison.OrdinalIgnoreCase)).ToList();
            if (FilesInFolder.Result.Any()) OrderedEntries.Insert(0, new Entry(Path));
            return OrderedEntries;
        }

        public async Task GetImagesAsync(string Path, SourceList<BitmapSource> imageList, CancellationToken token)
        {
            if (PathHelper.EnsureValidArchives(Path))
            {
                await Task.Run(() => GetImagesFromArchive(Path, imageList, token), token).ConfigureAwait(false);
            }
            else
            {
                await Task.Run(() => GetImagesFromDirectory(Path, imageList, token), token).ConfigureAwait(false);
            }
        }

        private void GetImagesFromDirectory(string Path, SourceList<BitmapSource> imageList, CancellationToken token)
        {
            try
            {
                var enumerable = Directory.EnumerateFiles(Path, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".jpg") || s.EndsWith(".png")).ToList();
                enumerable.Sort(new NaturalSortComparer(StringComparison.OrdinalIgnoreCase));
                for (var i = 0; i < enumerable.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    using Stream stream = File.Open(enumerable[i], FileMode.Open);
                    using var memoryStream = new MemoryStream();
                    GetImageFromStream(stream, imageList, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
                Console.WriteLine("Uncatched error in FetchImagesFromDirectory() function");
            }
        }

        private void GetImagesFromArchive(string Path, SourceList<BitmapSource> imageList, CancellationToken token)
        {
            try
            {
                using var archive = ArchiveFactory.Open(Path);
                var oderedArchive = archive.Entries.OrderBy(x => x.Key, new NaturalSortComparer(StringComparison.OrdinalIgnoreCase));
                foreach (var entry in oderedArchive)
                {
                    if (entry.IsDirectory || (!entry.Key.EndsWith("jpg") && !entry.Key.EndsWith("png") && !entry.Key.EndsWith("jpeg"))) continue;
                    token.ThrowIfCancellationRequested();
                    using var entryStream = entry.OpenEntryStream();
                    GetImageFromStream(entryStream, imageList, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Continue
            }
            catch (Exception)
            {
                Console.WriteLine("Uncatched error in FetchImages() function");
            }
        }

        private void GetImageFromStream(Stream stream, SourceList<BitmapSource> imageList, CancellationToken token)
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            token.ThrowIfCancellationRequested(); // start of expensive operation
            var bitmap = new Bitmap(memoryStream);
            var bitmapSource = ConvertStreamToSource(bitmap);
            bitmapSource.Freeze();
            token.ThrowIfCancellationRequested(); // end
            imageList.Add(bitmapSource);
            bitmap.Dispose();
        }
    

        private BitmapSource ConvertStreamToSource(Bitmap bitmap)
        {
            bitmap = CloneBitmap(bitmap);
            var bitmapsource = Convert(bitmap);
            return bitmapsource;
        }

        private Bitmap CloneBitmap(Bitmap x)
        {
            var PixelFormat = System.Drawing.Imaging.PixelFormat.Format16bppRgb555;

            Bitmap clone = new Bitmap(x.Width, x.Height, PixelFormat);

            using Graphics gr = Graphics.FromImage(clone);
            gr.InterpolationMode = Settings.Default.InterpolationMode;
            gr.PixelOffsetMode = Settings.Default.PixelOffsetMode;
            gr.SmoothingMode = Settings.Default.SmoothingMode;
            gr.Clear(Color.Transparent);
            gr.DrawImage(x, new Rectangle(0, 0, clone.Width, clone.Height));
            return clone;
        }

        private BitmapSource Convert(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                ConvertPixelFormat(bitmap.PixelFormat), null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            bitmap.Dispose();
            return bitmapSource;
        }

        private PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat sourceFormat)
        {
            switch (sourceFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormats.Bgr24;

                case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                    return PixelFormats.Pbgra32;

                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return PixelFormats.Bgra32;

                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return PixelFormats.Bgr32;

                case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
                    return PixelFormats.Bgr555;

                case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                    return PixelFormats.Bgr565;

                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return PixelFormats.Indexed8;

                    // .. add more if needed
            }

            return new PixelFormat();
        }
    }
}