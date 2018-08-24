using System;
using System.Collections.Generic;
using System.Linq;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.Models.Factories
{
    public class AllSongAlbumsGroupsFactory : SongsGroupsFactoryBase
    {
        public override List<SongsGroup> ClassifySongsGroups(IEnumerable<Song> allSongs)
        {
            var songsList = allSongs.ToList();
            var result = new List<SongsGroup>();

            // 统计专辑名称
            foreach (var song in songsList)
                if (result.All(sg => sg.Name != song.Album.Trim()))
                    result.Add(new SongsGroup(song.Album.Trim()));

            // 归纳数据
            foreach (var item in result)
            {
                item.Items.AddRange(songsList.Where(s => s.Album.Trim() == item.Name));

                item.AlbumCover = item.Items.First().AlbumCover;
            }

            return result;
        }
    }
}