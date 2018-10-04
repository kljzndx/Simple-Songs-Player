using System.Collections.Generic;
using System.Linq;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.Models.Factories
{
    public class AllSongsFoldersFactory : SongsGroupsFactoryBase
    {
        public override List<SongsGroup> ClassifySongsGroups(IEnumerable<Song> allSongs)
        {
            List<Song> sourceList = new List<Song>(allSongs);
            List<SongsGroup> songsGroups = new List<SongsGroup>();

            foreach (var song in sourceList)
                if (songsGroups.All(sg => sg.Name != song.FolderName))
                    songsGroups.Add(new SongsGroup(song.FolderName.Trim()));

            foreach (var songsGroup in songsGroups)
                songsGroup.Items.AddRange(sourceList.Where(s => s.FolderName.Trim() == songsGroup.Name));

            return songsGroups;
        }
    }
}