using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml.Media.Imaging;

namespace SimpleSongsPlayer.Models
{
    public class MusicAlbum : MusicGroup
    {
        public MusicAlbum(string name, IEnumerable<MusicUi> items) : base(name, items)
        {
        }

        public int Count => Items.Count();

        public Task<BitmapSource> GetCover()
        {
            return Items.First().GetCover();
        }
    }
}
