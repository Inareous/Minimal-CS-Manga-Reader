using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;

namespace Minimal_CS_Manga_Reader.Helper
{
    public class RegistryContextManager
    {
        private static readonly string hkcuFolderContext = @"Software\Classes\Directory\shell";
        private static readonly string MCSRegistry = @"MCSReader";
        private static readonly string commandSubDir = @"command";
        private static readonly string keyPath = $@"{hkcuFolderContext}\{MCSRegistry}";
        private static readonly string programPath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string fileExe = "Minimal CS Manga Reader.exe";
        private static readonly string fileIcon = "Minimal CS Manga Reader.ico";

        public static bool IsContextRegistry()
        {
            using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey($@"{keyPath}\{commandSubDir}", true);
            if (registryKey != null)
            {
                var str = registryKey?.GetValue(null).ToString();
                registryKey.Close();
                return str.Equals($"\"{programPath}Minimal CS Manga Reader.exe\" \"%L\" ");
            }
            else
            {
                return false; // Not found
            }
        }

        public static bool CreateContextRegistry()
        {
            if (EnsureFile())
            {
                return SetSubkey(EnsureIcon());
            }
            else
            {
                return false;
            }
        }

        private static bool SetSubkey(bool withIcon)
        {
            DeleteContextRegistry(); // Make sure we deal with clean slate, remove existing config
            using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(keyPath, true))
            {
                registryKey?.SetValue(null, $"Read with Minimal CS Manga Reader");
                if (withIcon) registryKey?.SetValue("icon", $"{programPath}{fileIcon}");
                registryKey?.Close();
            }

            using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey($@"{keyPath}\{commandSubDir}", true))
            {
                registryKey?.SetValue(null, $"\"{programPath}{fileExe}\" \"%L\" ");
                registryKey?.Close();
            }

            return true;
        }

        public static void DeleteContextRegistry()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree($@"{keyPath}", true);
                //Delete old relic too
                var oldKeyPath = @"SOFTWARE\Classes\Folder\shell\Minimal CS Manga Reader Context Menu";
                Registry.CurrentUser.DeleteSubKeyTree($@"{oldKeyPath}", true);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.ToString());
            }
        }

        private static bool EnsureFile()
        {
            var fileList = Directory
                .EnumerateFiles(programPath, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName);
            return fileList.Any(x => x.Equals(fileExe));
        }

        private static bool EnsureIcon()
        {
            var fileList = Directory
                .EnumerateFiles(programPath, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName);
            return fileList.Any(x => x.Equals(fileIcon));
        }

        public static void ChangeContextIntegrated(bool command)
        {
            if (command)
            {
                CreateContextRegistry();
            }
            else
            {
                DeleteContextRegistry();
            }
        }
    }
}
