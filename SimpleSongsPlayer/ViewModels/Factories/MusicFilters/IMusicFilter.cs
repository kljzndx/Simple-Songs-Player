using System.Collections.Generic;
using System.Collections.ObjectModel;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicFilters
{
    public interface IMusicFilter
    {
        IEnumerable<MusicFileDynamic> Filter(IEnumerable<MusicFileDTO> source, object constraint);
    }
}