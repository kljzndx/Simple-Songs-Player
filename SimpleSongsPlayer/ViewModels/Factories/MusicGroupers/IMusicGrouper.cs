using System.Collections.Generic;
using System.Collections.ObjectModel;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.Models;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicGroupers
{
    public interface IMusicGrouper
    {
        void Group(IEnumerable<MusicFile> source, ObservableCollection<MusicFileGroup> target);
    }
}