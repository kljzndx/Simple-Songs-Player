using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicGroupers
{
    public class MusicAlbumGrouper : IMusicGrouper
    {
        public async Task<IEnumerable<MusicFileGroup>> Group(IEnumerable<MusicFileDTO> source)
        {
            List<MusicFileGroup> groups = new List<MusicFileGroup>();

            foreach (var group in source.GroupBy(f => f.Album))
                 groups.Add(new MusicFileGroup(group.Key, group, f => f.GetAlbumCover()));

            return groups;
        }
    }
}