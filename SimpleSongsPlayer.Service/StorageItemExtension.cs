using System.Linq;
using Windows.Storage;

namespace SimpleSongsPlayer.Service
{
    public static class StorageItemExtension
    {
        public static string ToExtensionName(this IStorageItem item)
        {
            var strs = item.Name.Split('.');
            return strs.Last();
        }
    }
}