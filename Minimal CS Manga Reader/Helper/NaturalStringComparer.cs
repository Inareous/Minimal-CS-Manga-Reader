using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace Minimal_CS_Manga_Reader
{
    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }

    internal class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return SafeNativeMethods.StrCmpLogicalW(x, y);
        }
    }

    public sealed class NaturalFileInfoNameComparer : IComparer<FileInfo>
    {
        public int Compare(FileInfo x, FileInfo y)
        {
            return SafeNativeMethods.StrCmpLogicalW(x.Name, y.Name);
        }
    }

}