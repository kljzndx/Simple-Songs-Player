using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicGroupers
{
    public class MusicArtistGrouper : IMusicGrouper
    {
        public Task<IEnumerable<MusicFileGroup>> Group(IEnumerable<MusicFileDTO> source)
        {
            List<MusicFileGroup> groups = new List<MusicFileGroup>();

            foreach (var item in source.GroupBy(f => f.Artist))
                groups.Add(new MusicFileGroup(item.Key, item));

            return Task.FromResult<IEnumerable<MusicFileGroup>>(groups);
        }
    }
}