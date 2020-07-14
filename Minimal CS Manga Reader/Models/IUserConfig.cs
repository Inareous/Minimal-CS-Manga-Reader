using System.Drawing.Drawing2D;

namespace Minimal_CS_Manga_Reader.Models
{
    public interface IUserConfig
    {
        string Background { get; set; }
        bool FitImagesToScreen { get; set; }
        int ImageMargin { get; set; }
        InterpolationMode InterpolationMode { get; set; }
        bool IsScrollBarVisible { get; set; }
        string OpenChapterOnLoadChoice { get; set; }
        string Path { get; set; }
        PixelOffsetMode PixelOffsetMode { get; set; }
        int ScrollIncrement { get; set; }
        SmoothingMode SmoothingMode { get; set; }
        Enums.Theme Theme { get; set; }

        void Load();
        void Save();
    }
}