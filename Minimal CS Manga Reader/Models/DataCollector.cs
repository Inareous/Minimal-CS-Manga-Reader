﻿using DynamicData;
using Minimal_CS_Manga_Reader.Helper;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using PixelFormat = System.Windows.Media.PixelFormat;

namespace Minimal_CS_Manga_Reader
{
    internal class DataCollector
    {
        internal async Task<IEnumerable<string>> GetChapterListAsync(string Path, bool IsArchive)
        {
            if (IsArchive) return new List<string> { Path };
            var Files = Task.Run(() => Directory.EnumerateFiles(Path, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(s => s.EndsWith(".cbz") || s.EndsWith(".cbr") || s.EndsWith(".rar") || s.EndsWith(".zip")));
            var Folders = Task.Run(() => Directory.EnumerateDirectories(Path, "*", SearchOption.TopDirectoryOnly));
            await Task.WhenAll(Files, Folders).ConfigureAwait(false);
            var ReturnList = (Files.Result ?? Enumerable.Empty<string>()).Concat(Folders.Result ?? Enumerable.Empty<string>()).ToList();
            ReturnList.Sort(new NaturalStringComparer());
            return ReturnList;
        }

        internal async Task GetImagesAsync(string Path, SourceList<BitmapSource> imageList, CancellationToken token)
        {
            if (!ZipArchive.IsZipFile(Path) && !RarArchive.IsRarFile(Path))
            {
                await Task.Run(() => GetImagesFromDirectory(Path, imageList, token), token).ConfigureAwait(false);
            }
            else
            {
                await Task.Run(() => GetImagesFromArchive(Path, imageList, token), token).ConfigureAwait(false);
            }
        }

        private void GetImagesFromDirectory(string Path, SourceList<BitmapSource> imageList, CancellationToken token)
        {
            try
            {
                var enumerable = Directory.EnumerateFiles(Path, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".jpg") || s.EndsWith(".png")).ToList();
                enumerable.Sort(new NaturalStringComparer());
                for (var i = 0; i < enumerable.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    using Stream stream = File.Open(enumerable[i], FileMode.Open);
                    using var memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    token.ThrowIfCancellationRequested();
                    Bitmap bitmap = new Bitmap(memoryStream);
                    BitmapSource bitmapSource = ConvertStreamToSource(bitmap);
                    bitmapSource.Freeze();
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        imageList.Add(bitmapSource);
                        bitmap.Dispose();
                    });
                }
            }
            catch (OperationCanceledException)
            {
                Application.Current.Dispatcher.Invoke(delegate
                { imageList.Clear(); });
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
                using Stream stream = File.Open(Path, FileMode.Open);
                using var reader = SharpCompress.Readers.ReaderFactory.Open(stream);
                while (reader.MoveToNextEntry())
                {
                    if (reader.Entry.IsDirectory || (!reader.Entry.Key.EndsWith("jpg") && !reader.Entry.Key.EndsWith("png") && !reader.Entry.Key.EndsWith("jpeg"))) continue;
                    token.ThrowIfCancellationRequested();
                    using var entryStream = reader.OpenEntryStream();
                    using var memoryStream = new MemoryStream();
                    entryStream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    token.ThrowIfCancellationRequested();
                    var bitmap = new Bitmap(memoryStream);
                    var bitmapSource = ConvertStreamToSource(bitmap);
                    bitmapSource.Freeze();
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        imageList.Add(bitmapSource);
                        bitmap.Dispose();
                    });
                }
            }
            catch (OperationCanceledException)
            {
                Application.Current.Dispatcher.Invoke(delegate
                { imageList.Clear(); });
            }
            catch (Exception)
            {
                Console.WriteLine("Uncatched error in FetchImages() function");
            }
        }

        private BitmapSource ConvertStreamToSource(Bitmap x)
        {
            var PixelFormat = System.Drawing.Imaging.PixelFormat.Format16bppRgb555;

            Bitmap clone = new Bitmap(x.Width, x.Height, PixelFormat);

            using Graphics gr = Graphics.FromImage(clone);
            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
            // gr.SmoothingMode = SmoothingMode.HighQuality;
            gr.Clear(Color.Transparent);
            gr.DrawImage(x, new Rectangle(0, 0, clone.Width, clone.Height));
            return Convert(clone);
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
    }
}