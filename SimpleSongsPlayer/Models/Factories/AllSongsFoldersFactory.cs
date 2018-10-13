using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.Models.Factories
{
    public class AllSongsFoldersFactory : ISongsGroupsFactory
    {
        public ObservableCollection<SongsGroup> ClassifySongGroups(IEnumerable<Song> allSongs)
        {
            List<Song> sourceList = new List<Song>(allSongs);
            var songsGroups = new ObservableCollection<SongsGroup>();

            foreach (var song in sourceList)
                if (songsGroups.All(sg => sg.Name != song.FolderName))
                    songsGroups.Add(new SongsGroup(song.FolderName.Trim()));

            foreach (var songsGroup in songsGroups)
                foreach (var song in sourceList.Where(s => s.FolderName.Trim() == songsGroup.Name))
                    songsGroup.Items.Add(song);

            return songsGroups;
        }
    }
}