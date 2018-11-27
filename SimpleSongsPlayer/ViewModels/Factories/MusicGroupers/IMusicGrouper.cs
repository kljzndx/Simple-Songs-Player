using System.Collections.Generic;
using System.Collections.ObjectModel;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.Models;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicGroupers
{
    public interface IMusicGrouper
    {
        IEnumerable<MusicFileGroup> Group(IEnumerable<MusicFile> source);
    }
}