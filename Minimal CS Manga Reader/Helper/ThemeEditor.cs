using MaterialDesignThemes.Wpf;
using Minimal_CS_Manga_Reader.Models;

namespace Minimal_CS_Manga_Reader.Helper
{
    public class ThemeEditor
    {
        public static void ModifyTheme(Enums.Theme _theme)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

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
    }
}
