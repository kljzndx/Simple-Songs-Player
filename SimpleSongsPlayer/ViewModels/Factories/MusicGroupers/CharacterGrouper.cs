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
            var result = cgs.Where(c => !String.IsNullOrWhiteSpace(c.Label) && !c.Label.Contains("拼音")).Select(c => new MusicFileGroup(c.Label.Trim())).ToList();

            foreach (var fileDto in source)
            {
                string label = cgs.Lookup(fileDto.Title).Replace("拼音", String.Empty).Trim();
                MusicFileGroup theGroup = null;
                if (result.Find(g => g.Name == label) is MusicFileGroup group)
                    theGroup = group;
                else
                {
                    theGroup = new MusicFileGroup(label);
                    result.Add(theGroup);
                }

                theGroup.Items.Add(fileDto);
            }

            return Task.FromResult<IEnumerable<MusicFileGroup>>(result);
        }
    }
}