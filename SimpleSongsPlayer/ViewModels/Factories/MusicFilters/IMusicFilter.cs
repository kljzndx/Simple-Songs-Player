using System.Collections.Generic;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicFilters
{
    public interface IMusicFilter
    {
        IEnumerable<MusicFileDTO> Filter(IEnumerable<MusicFileDTO> source, object constraint);
    }
}