using MaterialDesignColors;
using MaterialDesignColors.ColorManipulation;
using MaterialDesignThemes.Wpf;
using Minimal_CS_Manga_Reader.Models;
using System.Windows.Media;

namespace Minimal_CS_Manga_Reader.Helper
{
    public class ThemeEditor
    {
        public static PaletteHelper paletteHelper = new PaletteHelper();

        public static ITheme theme = paletteHelper.GetTheme();

        public static void ModifyTheme(Enums.Theme _theme)
        {
            if (_theme == Enums.Theme.Dark)
            {
                theme.SetBaseTheme(Theme.Dark);
            }
            else
            {
                theme.SetBaseTheme(Theme.Light);
            }

            paletteHelper.SetTheme(theme);
        }

        public static void ChangePrimaryColor(Color color)
        {
            theme.PrimaryLight = new ColorPair(color.Lighten(), theme.PrimaryLight.ForegroundColor);
            theme.PrimaryMid = new ColorPair(color, theme.PrimaryMid.ForegroundColor);
            theme.PrimaryDark = new ColorPair(color.Darken(), theme.PrimaryDark.ForegroundColor);

            paletteHelper.SetTheme(theme);
        }

        public static void ChangeSecondaryColor(Color color)
        {
            theme.SecondaryLight = new ColorPair(color.Lighten(), theme.SecondaryLight.ForegroundColor);
            theme.SecondaryMid = new ColorPair(color, theme.SecondaryMid.ForegroundColor);
            theme.SecondaryDark = new ColorPair(color.Darken(), theme.SecondaryDark.ForegroundColor);

            paletteHelper.SetTheme(theme);
        }
    }
}
