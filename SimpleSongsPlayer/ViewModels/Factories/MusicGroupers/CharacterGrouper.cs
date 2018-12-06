using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization.Collation;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicGroupers
{
    public class CharacterGrouper : IMusicGrouper
    {
        private static readonly CharacterGroupings cgs = new CharacterGroupings();

        public Task<IEnumerable<MusicFileGroup>> Group(IEnumerable<MusicFileDTO> source)
        {
            var result = cgs.Where(c => !String.IsNullOrWhiteSpace(c.Label) && !c.Label.Contains("拼音")).Select(c => new MusicFileGroup(c.Label)).ToList();

            foreach (var fileDto in source)
            {
                string label = cgs.Lookup(fileDto.Title).Replace("拼音", String.Empty);
                result.Find(g => g.Name == label).Items.Add(fileDto);
            }

            return Task.FromResult<IEnumerable<MusicFileGroup>>(result);
        }
    }
}