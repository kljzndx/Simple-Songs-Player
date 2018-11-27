using System.Collections.Generic;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicGroupers
{
    public interface IMusicGrouper
    {
        IEnumerable<MusicFileGroup> Group(IEnumerable<MusicFileDTO> source);
    }
}