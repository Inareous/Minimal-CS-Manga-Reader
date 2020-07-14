using DynamicData;
using Minimal_CS_Manga_Reader.Helper;
using Minimal_CS_Manga_Reader.Models;
using SharpCompress.Archives;
using Splat;
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
        private readonly IUserConfig Config; 
        public DataCollector(IUserConfig config)
        {
            Config = config ?? Locator.Current.GetService<IUserConfig>();
        }

        public async Task<IEnumerable<Entry>> GetChapterListAsync(string Path, bool IsArchive)
        {
            if (IsArchive) return new List<Entry> { new Entry(Path) };
            var FilesInFolder = Task.Run(() => Directory.EnumerateFiles(Path, "*.*", SearchOption.TopDirectoryOnly)
                                                        .Where(s => PathHelper.EnsureAcceptedImageTypes(s) != Enums.ImageType.NotImage));
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

        public void GetImages(string Path, SourceList<BitmapSource> imageList, CancellationToken token)
        {
            if (PathHelper.EnsureValidArchives(Path))
            {
                GetImagesFromArchive(Path, imageList, token);
            }
            else
            {
                GetImagesFromDirectory(Path, imageList, token);
            }
        }

        private void GetImagesFromDirectory(string Path, SourceList<BitmapSource> imageList, CancellationToken token)
        {
            try
            {
                var enumerable = Directory.EnumerateFiles(Path, "*.*", SearchOption.TopDirectoryOnly)
                                          .Where(s => PathHelper.EnsureAcceptedImageTypes(s) != Enums.ImageType.NotImage).ToList();
                enumerable.Sort(new NaturalSortComparer(StringComparison.OrdinalIgnoreCase));
                for (var i = 0; i < enumerable.Count; i++)
                {
                    var imageType = PathHelper.EnsureAcceptedImageTypes(enumerable[i]);
                    token.ThrowIfCancellationRequested();
                    using Stream stream = File.Open(enumerable[i], FileMode.Open);
                    GetImageFromStream(stream, imageType, imageList, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine($"Uncatched error in FetchImagesFromDirectory() function ({e})");
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
                    var imageType = PathHelper.EnsureAcceptedImageTypes(entry.Key);
                    if (entry.IsDirectory || imageType == Enums.ImageType.NotImage) continue;
                    token.ThrowIfCancellationRequested();
                    using var entryStream = entry.OpenEntryStream();
                    GetImageFromStream(entryStream, imageType, imageList, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Continue
            }
            catch (Exception e)
            {
                Console.WriteLine($"Uncatched error in GetImagesFromArchive() function ({e})");
            }
        }

        private void GetImageFromStream(Stream stream, Enums.ImageType imageType, SourceList<BitmapSource> imageList, CancellationToken token)
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            token.ThrowIfCancellationRequested(); // start of expensive operation
            Bitmap bitmap;
            switch (imageType)
            {
                case Enums.ImageType.Default:
                    bitmap = new Bitmap(memoryStream);
                    break;
                case Enums.ImageType.WebP:
                    byte[] b = memoryStream.ToArray();
                    using (WebP webp = new WebP())
                        bitmap = webp.Decode(b);
                    break;
                default: throw new BadImageFormatException();
            }
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
            gr.InterpolationMode = Config.InterpolationMode;
            gr.PixelOffsetMode = Config.PixelOffsetMode;
            gr.SmoothingMode = Config.SmoothingMode;
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