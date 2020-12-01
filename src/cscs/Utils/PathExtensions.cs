using System;
using System.IO;
using System.Linq;
using System.Threading;

#if class_lib

namespace CSScriptLib
#else

namespace csscript
#endif
{
    /// <summary>
    /// Various PATH extensions
    /// </summary>
    static class PathExtensions
    {
        public static string GetExtension(this string path) => Path.GetExtension(path);

        public static string GetFileName(this string path) => Path.GetFileName(path);

        public static bool DirExists(this string path) => path.IsNotEmpty() ? Directory.Exists(path) : false;

        public static string GetFullPath(this string path) => Path.GetFullPath(path);

        public static string PathJoin(this string path, params object[] parts)
        {
            var allParts = new[] { path ?? "" }.Concat(parts.Select(x => x?.ToString() ?? ""));
            return Path.Combine(allParts.ToArray());
        }

        public static string GetPath(this Environment.SpecialFolder folder)
        {
            return Environment.GetFolderPath(folder);
        }

        public static string EnsureDir(this string path)
        {
            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetDirName(this string path)
            => path == null ? null : Path.GetDirectoryName(path);

        public static string ChangeFileName(this string path, string fileName) => path.GetDirName().PathJoin(fileName);

        public static string GetFileNameWithoutExtension(this string path) => Path.GetFileNameWithoutExtension(path);

        public static string PathNormaliseSeparators(this string path)
        {
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

#if !class_lib

        public static bool IsDirSectionSeparator(this string text)
        {
            return text != null && text.StartsWith(Settings.dirs_section_prefix) && text.StartsWith(Settings.dirs_section_suffix);
        }

#endif
    }
}