﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicGroupers
{
    public class MusicAlbumGrouper : IMusicGrouper
    {
        public IEnumerable<MusicFileGroup> Group(IEnumerable<MusicFileDTO> source)
        {
            foreach (var group in source.GroupBy(f => f.Album))
                yield return new MusicFileGroup(group.Key, group, f => f.GetAlbumCover());
        }
    }
}