using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace IntegrateContext
{
    internal static class Program
    {
        private static void Main()
        {
            var curPath = Directory.GetCurrentDirectory();
            //RegistryAccess.ReadSubKeyValue(curPath);
            // Exiting
            int index = RegistryAccess.ReadSubKeyValue(curPath);
            Console.Write("Integrator Context for Minimal CS Manga Reader" + "\n\n" +
                $"Current Status : ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(status[index] + "\n");
            Console.ResetColor();
            Console.WriteLine(
                $"Your option : \n" +
                $"1. Create/Update the registry" + "\n" +
                $"2. Delete registry\n");
            Console.Write($"Enter your input: ");
            var input = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine("");
            if (int.TryParse(input, out int result) && result > 0 && result <= 2)
            {
                switch (RegistryAccess.EditRegistry(result))
                {
                    case 1:
                        Console.WriteLine("Registry created/updated");
                        break;

                    case 2:
                        Console.WriteLine("Registry deleted");
                        break;

                    default:
                        Console.WriteLine("Something went wrong!");
                        break;
                }
                Console.WriteLine("Exiting . . .");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Wrong input, exiting. . .");
            }

            Thread.Sleep(3000);
            Environment.Exit(0);
        }

        public static Dictionary<int, string> status { get; set; } = new Dictionary<int, string>(){
            {1,"Registry exist but in different path. Try updating the registry"},
            {2,"Registry exist already"},
            {3,"Registry not found"}
            };
    }

    internal static class RegistryAccess
    {
        private static string hkcuFolderContext = @"SOFTWARE\Classes\Folder\shell";
        private static string MCSRegistry = @"Minimal CS Manga Reader Context Menu";
        private static string commandSubDir = @"command";
        private static string keyPath = $@"{hkcuFolderContext}\{MCSRegistry}";
        private static string programPath { get; set; }

        public static bool CheckFile(string curPath)
        {
            var fileList = Directory
                .EnumerateFiles(curPath, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName); // <-- note you can shorten the lambda
            return fileList.Any(x => x.Equals("Minimal CS Manga Reader.exe")) ? true : false;
        }

        public static bool CheckIcon(string curPath)
        {
            var fileList = Directory
                .EnumerateFiles(curPath, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName); // <-- note you can shorten the lambda
            return fileList.Any(x => x.Equals("Minimal CS Manga Reader.ico")) ? true : false;
        }

        public static int ReadSubKeyValue(string curPath)
        {
            programPath = curPath;
            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey($@"{keyPath}\{commandSubDir}", true))
            {
                if (registryKey != null)
                {
                    var str = registryKey?.GetValue(null).ToString();
                    registryKey.Close();
                    return !str.Equals($"\"{curPath}\\Minimal CS Manga Reader.exe\" \"-path=%L\" ") ? 1 : 2;
                }
                else
                {
                    return 3; // Not found
                }
            }
        }

        private static void DeleteAllKey()
        {
            Registry.CurrentUser.DeleteSubKeyTree($@"{keyPath}", true);
        }

        private static bool CreateSubKeyValue()
        {
            if (CheckFile(programPath) && CheckIcon(programPath))
            {
                var fileExe = "Minimal CS Manga Reader.exe";
                var fileIcon = "Minimal CS Manga Reader.ico";
                using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(keyPath, true))
                {
                    registryKey?.SetValue(null, $"Read with Minimal CS Manga Reader");
                    registryKey?.SetValue("icon", $"{programPath}\\{fileIcon}");
                    registryKey?.Close();
                }

                using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey($@"{keyPath}\{commandSubDir}", true))
                {
                    registryKey?.SetValue(null, $"\"{programPath}\\{fileExe}\" \"-path=%L\" ");
                    registryKey?.Close();
                }

                return true;
            }
            else
            {
                Console.WriteLine(
                    "File and icon missing, please make sure you place IntegrateContext in the same folder as Minimal CS Manga Reader.exe and Minimal CS Manga Reader.ico\n" +
                    "Exiting. .");
                return false;
            }
        }

        internal static int EditRegistry(int result)
        {
            switch (result)
            {
                case 1:
                    bool state = CreateSubKeyValue();
                    return state ? 1 : 0;

                case 2:
                    DeleteAllKey();
                    return 2;

                default:
                    return 0;
            }
        }
    }
}