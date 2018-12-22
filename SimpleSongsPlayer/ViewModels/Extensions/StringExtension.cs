using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Gaming.Input;

namespace SimpleSongsPlayer.ViewModels.Extensions
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
    }
}