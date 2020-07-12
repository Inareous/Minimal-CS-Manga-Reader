using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Minimal_CS_Manga_Reader.Models
{
    public class BookmarksSource : IBookmarksSource
    {
        private const string fileName = "bookmarks.json";
        public SourceList<Bookmark> Bookmarks { get; set; } = new SourceList<Bookmark>();
        public async Task LoadAsync()
        {
            try
            {
                using FileStream fs = File.OpenRead(fileName);
                var bookmarkList = await JsonSerializer.DeserializeAsync<List<Bookmark>>(fs);
                Bookmarks.AddRange(bookmarkList);
            }
            catch (FileNotFoundException)
            {
                Bookmarks = new SourceList<Bookmark>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public async Task SaveAsync()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            var x = Bookmarks.Connect().Bind(out ReadOnlyObservableCollection<Bookmark> list).DisposeMany().Subscribe();
            var jsonString = JsonSerializer.Serialize(list.ToList(), options);
            await File.WriteAllTextAsync(fileName, jsonString);
            x.Dispose();
        }
        public void Add(Bookmark bookmark)
        {
            var x = Bookmarks.AsObservableList().Connect()
                .Filter(x => x.Equals(bookmark))
                .Bind(out ReadOnlyObservableCollection<Bookmark> list)
                .DisposeMany()
                .Subscribe();
            if (list.Count == 0) Bookmarks.Add(bookmark);
            x.Dispose();
        }
        public void Delete(Bookmark bookmark)
        {
            try
            {
                Bookmarks.Remove(bookmark);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print("Unable to delete bookmark");
                System.Diagnostics.Debug.Print(e.ToString());
            }
        }
    }
}
