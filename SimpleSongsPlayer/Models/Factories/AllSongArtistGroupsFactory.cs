using System;
using System.Collections.Generic;
using System.Linq;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.Models.Factories
{
    public class AllSongArtistGroupsFactory : SongsGroupsFactoryBase
    {
        public override List<SongsGroup> ClassifySongsGroups(IEnumerable<Song> allSongs)
        {
            var songsList = allSongs.ToList();
            var result = new List<SongsGroup>();

            foreach (var song in songsList)
                if (result.All(sg => sg.Name != song.Singer.Trim()))
                    result.Add(new SongsGroup(song.Singer.Trim()));

            foreach (var item in result)
                item.Items.AddRange(songsList.Where(s => s.Singer.Trim() == item.Name));

            return result;
        }
    }
}