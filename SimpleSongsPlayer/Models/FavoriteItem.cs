using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.Models
{
    public class FavoriteItem
    {
        private FavoriteItem(PlayingListBlock block, StorageItemThumbnail thumbnail)
        {
            Name = block.Name;
            Count = block.Count;
            BitmapImage image = new BitmapImage();
            image.SetSource(thumbnail);
            Cover = image;
        }

        public string Name { get; private set; }
        public int Count { get; private set; }
        public BitmapSource Cover { get; private set; }

        public static async Task<FavoriteItem> FromPlayingListBlock(PlayingListBlock block)
        {
            var paths = await block.GetPaths();
            var coverSource = await (await StorageFile.GetFileFromPathAsync(paths.First())).GetThumbnailAsync(ThumbnailMode.MusicView);
            return new FavoriteItem(block, coverSource);
        }
    }
}