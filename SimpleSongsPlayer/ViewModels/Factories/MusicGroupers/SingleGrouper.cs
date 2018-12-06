using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicGroupers
{
    public class SingleGrouper : IMusicGrouper
    {
        public Task<IEnumerable<MusicFileGroup>> Group(IEnumerable<MusicFileDTO> source)
        {
            return Task.FromResult<IEnumerable<MusicFileGroup>>(new[] {new MusicFileGroup(String.Empty, source)});
        }
    }
}