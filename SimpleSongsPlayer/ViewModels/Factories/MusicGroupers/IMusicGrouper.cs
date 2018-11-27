using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicGroupers
{
    public interface IMusicGrouper
    {
        Task<IEnumerable<MusicFileGroup>> Group(IEnumerable<MusicFileDTO> source);
    }
}