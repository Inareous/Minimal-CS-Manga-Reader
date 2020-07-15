using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Minimal_CS_Manga_Reader.Models
{
    public class UserConfig : IUserConfig
    {
        private const string userFile = "UserConfig.json";
        public string Path { get; set; } = "FirstTimeNotSet";
        public int ScrollIncrement { get; set; } = 100;
        public int ImageMargin { get; set; } = 20;
        public string Background { get; set; } = "Silver";
        public System.Drawing.Drawing2D.InterpolationMode InterpolationMode { get; set;}
        public System.Drawing.Drawing2D.SmoothingMode SmoothingMode { get; set; }
        public System.Drawing.Drawing2D.PixelOffsetMode PixelOffsetMode { get; set; }
        public bool FitImagesToScreen { get; set; } = false;
        public Enums.OpenChapterOnLoad OpenChapterOnLoadChoice { get; set; } = Enums.OpenChapterOnLoad.Last;
        public bool IsScrollBarVisible { get; set; } = true;
        public Enums.Theme Theme { get; set; } = Enums.Theme.Light;
        public void Load()
        {
            // To do : Create a more robust User Setting parser
            try
            {
                var jsonstring = File.ReadAllText(userFile);
                var JsonConfig = JsonSerializer.Deserialize<UserConfig>(jsonstring);
                Path = JsonConfig.Path;
                ScrollIncrement = JsonConfig.ScrollIncrement;
                ImageMargin = JsonConfig.ImageMargin;
                Background = JsonConfig.Background;
                FitImagesToScreen = JsonConfig.FitImagesToScreen;
                OpenChapterOnLoadChoice = JsonConfig.OpenChapterOnLoadChoice;
                IsScrollBarVisible = JsonConfig.IsScrollBarVisible;
                if (Enum.IsDefined(typeof(System.Drawing.Drawing2D.InterpolationMode), JsonConfig.InterpolationMode))
                    InterpolationMode = JsonConfig.InterpolationMode;
                if (Enum.IsDefined(typeof(System.Drawing.Drawing2D.SmoothingMode), JsonConfig.SmoothingMode))
                    SmoothingMode = JsonConfig.SmoothingMode;
                if (Enum.IsDefined(typeof(System.Drawing.Drawing2D.PixelOffsetMode), JsonConfig.PixelOffsetMode))
                    PixelOffsetMode = JsonConfig.PixelOffsetMode;
                if (Enum.IsDefined(typeof(Enums.Theme), JsonConfig.Theme))
                    Theme = JsonConfig.Theme;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public void Save()
        {
            var jsonString = JsonSerializer.Serialize(new UserConfig
            {
                Path = Path,
                ScrollIncrement = ScrollIncrement,
                ImageMargin = ImageMargin,
                Background = Background,
                InterpolationMode = InterpolationMode,
                SmoothingMode = SmoothingMode,
                PixelOffsetMode = PixelOffsetMode,
                FitImagesToScreen = FitImagesToScreen,
                OpenChapterOnLoadChoice = OpenChapterOnLoadChoice,
                IsScrollBarVisible = IsScrollBarVisible,
                Theme = Theme
            });
            File.WriteAllText(userFile, jsonString);
        }
    }
}
