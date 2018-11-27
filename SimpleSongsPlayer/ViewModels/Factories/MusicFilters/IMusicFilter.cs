using System.Collections.Generic;
using System.Collections.ObjectModel;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicFilters
{
    public interface IMusicFilter
    {
        void Filter(IEnumerable<MusicFileDTO> source, ObservableCollection<MusicFileDynamic> target);
    }
}