using System;
using System.Collections.Generic;
using System.Linq;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicFilters
{
    public class MusicArtistFilter : IMusicFilter
    {
        public IEnumerable<MusicFileDynamic> Filter(IEnumerable<MusicFileDTO> source, object constraint)
        {
            if (!(constraint is string str))
                throw new Exception("请传入作者名");

            foreach (var file in source.Where(f => f.Artist == str))
                yield return new MusicFileDynamic(file);
        }
    }
}