using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicGroupers
{
    public class MusicArtistGrouper : IMusicGrouper
    {
        public IEnumerable<MusicFileGroup> Group(IEnumerable<MusicFileDTO> source)
        {
            foreach (var item in source.GroupBy(f => f.Artist))
                yield return new MusicFileGroup(item.Key, item, "ms-appx:///Assets/Icons/Artist.png");
        }
    }
}