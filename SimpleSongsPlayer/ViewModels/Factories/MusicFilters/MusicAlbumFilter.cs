using System;
using System.Collections.Generic;
using System.Linq;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicFilters
{
    public class MusicAlbumFilter : IMusicFilter
    {
        public IEnumerable<MusicFileDTO> Filter(IEnumerable<MusicFileDTO> source, object constraint)
        {
            if (!(constraint is string str))
                throw new Exception("请传入专辑名");

            foreach (var fileDto in source.Where(f => f.Album == str))
                yield return fileDto;
        }
    }
}