using System;
using System.Linq;

namespace SimpleSongsPlayer.DAL.Factory
{
    public static class StringExtension
    {
        public static string TrimExtensionName(this string str)
        {
            var blocks = str.Split('.');
            var result = blocks.ToList();
            if (result.Count > 1)
                result.Remove(result.Last());

            return String.Join(".", result);
        }

        public static string TakeParentPath(this string path)
        {
            var paths = path.Split('\\').ToList();
            paths.Remove(paths.Last());
            return String.Join("\\", paths);
        }
    }
}