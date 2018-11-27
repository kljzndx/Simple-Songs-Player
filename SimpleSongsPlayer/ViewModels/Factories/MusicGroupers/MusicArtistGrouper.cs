using System.Collections.Generic;
using System.Linq;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicGroupers
{
    public class MusicArtistGrouper : IMusicGrouper
    {
        public IEnumerable<MusicFileGroup> Group(IEnumerable<MusicFileDTO> source)
        {
            foreach (var item in source.GroupBy(f => f.Artist))
                yield return new MusicFileGroup(item.Key, item);
        }
    }
}