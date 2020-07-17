using System;
using System.IO;
using System.Text.Json;
using System.Drawing.Drawing2D;
using System.Windows.Media;

namespace Minimal_CS_Manga_Reader.Models
{
    public class UserConfig : IUserConfig
    {
        private readonly string userFile = $@"{AppDomain.CurrentDomain.BaseDirectory}\UserConfig.json";
        public string Path { get; set; } = "FirstTimeNotSet";
        public int ScrollIncrement { get; set; } = 100;
        public int ImageMargin { get; set; } = 20;
        public string Background { get; set; } = "Silver";
        public InterpolationMode InterpolationMode { get; set;}
        public SmoothingMode SmoothingMode { get; set; }
        public PixelOffsetMode PixelOffsetMode { get; set; }
        public bool FitImagesToScreen { get; set; } = false;
        public Enums.OpenChapterOnLoad OpenChapterOnLoadChoice { get; set; } = Enums.OpenChapterOnLoad.Last;
        public bool IsScrollBarVisible { get; set; } = true;
        public Enums.Theme Theme { get; set; } = Enums.Theme.Light;
        public Color AccentColor { get; set; } = Color.FromArgb(255, 154, 103, 234);
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
                AccentColor = JsonConfig.AccentColor;
                if (Enum.IsDefined(typeof(InterpolationMode), JsonConfig.InterpolationMode))
                    InterpolationMode = JsonConfig.InterpolationMode;
                if (Enum.IsDefined(typeof(SmoothingMode), JsonConfig.SmoothingMode))
                    SmoothingMode = JsonConfig.SmoothingMode;
                if (Enum.IsDefined(typeof(PixelOffsetMode), JsonConfig.PixelOffsetMode))
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
                Theme = Theme,
                AccentColor = AccentColor
            });
            File.WriteAllText(userFile, jsonString);
        }
    }
}
