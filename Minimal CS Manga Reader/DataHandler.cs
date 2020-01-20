using DynamicData;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using PixelFormat = System.Windows.Media.PixelFormat;

namespace Minimal_CS_Manga_Reader
{
    public static class DataHandler
    {
        public static List<string> FetchChapters(string path)
        {
            // TO DO -- OPTIMIZE
            var fileList = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly).Where(s =>
                s.EndsWith(".cbz") || s.EndsWith(".cbr") || s.EndsWith(".rar") || s.EndsWith(".zip"));
            var directories = Directory.GetDirectories(path);
            var returnList = new List<string>();
            returnList.AddRange(fileList);
            returnList.AddRange(directories);
            returnList.Sort(new NaturalStringComparer());
            return returnList;
        }

        public static void FetchImages(string file, SourceList<BitmapSource> xBitmaps, CancellationToken token)
        {
            try
            {
                if (!ZipArchive.IsZipFile(file) && !RarArchive.IsRarFile(file))
                {
                    FetchImagesFromDirectory(file, xBitmaps, token);
                }
                else
                {
                    using Stream stream = File.Open(file, FileMode.Open);
                    using var reader = ReaderFactory.Open(stream);
                    while (reader.MoveToNextEntry())
                    {
                        if (reader.Entry.IsDirectory || (!reader.Entry.Key.EndsWith("jpg") && !reader.Entry.Key.EndsWith("png") && !reader.Entry.Key.EndsWith("jpeg"))) continue;
                        token.ThrowIfCancellationRequested();
                        using var entryStream = reader.OpenEntryStream();
                        using var memoryStream = new MemoryStream();
                        entryStream.CopyTo(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        token.ThrowIfCancellationRequested();
                        var x = ConvertStreamToSource(new Bitmap(memoryStream));
                        x.Freeze();
                        Application.Current.Dispatcher.Invoke(delegate // <--- Update from UI Thread
                        { xBitmaps.Add(x); });
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Application.Current.Dispatcher.Invoke(delegate // <--- Update from UI Thread
                { xBitmaps.Clear(); });
            }
            catch (Exception)
            {
                Console.WriteLine("Uncatched error in FetchImages() function");
            }
        }

        private static void FetchImagesFromDirectory(string path, SourceList<BitmapSource> xBitmaps,
           CancellationToken token)
        {
            try
            {
                var enumerable = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".jpg") || s.EndsWith(".png")).ToList();
                enumerable.Sort(new NaturalStringComparer());
                for (var i = 0; i < enumerable.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    using Stream stream = File.Open(enumerable[i], FileMode.Open);
                    using var memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    token.ThrowIfCancellationRequested();
                    var x = ConvertStreamToSource(new Bitmap(memoryStream));
                    x.Freeze();
                    Application.Current.Dispatcher.Invoke(delegate // <--- Update from UI Thread
                    { xBitmaps.Add(x); });
                }
            }
            catch (OperationCanceledException)
            {
                Application.Current.Dispatcher.Invoke(delegate // <--- Update from UI Thread
                { xBitmaps.Clear(); });
            }
            catch (Exception)
            {
                Console.WriteLine("Uncatched error in FetchImagesFromDirectory() function");
            }
        }

        private static PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat sourceFormat)
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

        private static BitmapSource Convert(Bitmap bitmap)
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

        private static BitmapSource ConvertStreamToSource(Bitmap x)
        {
            var lowPixelFormat = System.Drawing.Imaging.PixelFormat.Format16bppRgb555;

            Bitmap clone = new Bitmap(x.Width, x.Height, lowPixelFormat);

            using Graphics gr = Graphics.FromImage(clone);
            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
            // gr.SmoothingMode = SmoothingMode.HighQuality;
            gr.Clear(Color.Transparent);
            gr.DrawImage(x, new Rectangle(0, 0, clone.Width, clone.Height));
            x.Dispose();
            return Convert(clone);
        }

        /*
        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using var memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
        */

        /*
        private static BitmapSource ConvertStreamToSource(Bitmap x)
        {
            var dpiYProperty =
                typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
            if (dpiYProperty == null) return null;
            var dpiY = (int)dpiYProperty.GetValue(null, null);
            var factor = dpiY / 96f;
            var width = (int)Math.Round(x.Width * factor);
            var height = (int)Math.Round(x.Height * factor);
            var lowResPixelFormat = System.Drawing.Imaging.PixelFormat.Format16bppRgb555;
            Bitmap newBitmap;
            // Create bitmaps.
            try
            {
                newBitmap = new Bitmap(width, height, lowResPixelFormat);
                using var g = Graphics.FromImage(newBitmap);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.Clear(Color.Transparent);
                g.DrawImage(x, 0, 0, newBitmap.Width, newBitmap.Height);
            }
            catch (Exception e) when (e.Message.Equals(
                    "A Graphics object cannot be created from an image that has an indexed pixel format."))
            {
                // Catch "A Graphics object cannot be created from an image that has an indexed pixel format."
                newBitmap = new Bitmap(width, height, lowResPixelFormat);
                using var g = Graphics.FromImage(newBitmap);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.Clear(Color.Transparent);
                g.DrawImage(x, 0, 0, newBitmap.Width, newBitmap.Height);
            }
            return Convert(newBitmap); // Return
        }
        */
    }
}