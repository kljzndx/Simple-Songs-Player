using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.Models.Factories
{
    public class AllSongArtistGroupsFactory : ISongsGroupsFactory
    {
        public ObservableCollection<SongsGroup> ClassifySongGroups(IEnumerable<Song> allSongs)
        {
            var songsList = allSongs.ToList();
            var result = new ObservableCollection<SongsGroup>();

            foreach (var song in songsList)
                if (result.All(sg => sg.Name != song.Singer.Trim()))
                    result.Add(new SongsGroup(song.Singer.Trim()));

            foreach (var item in result)
                foreach (var song in songsList.Where(s => s.Singer.Trim() == item.Name))
                    item.Items.Add(new SongItem(song));

            return result;
        }
    }
}