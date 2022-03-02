using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.Models
{
    public class MusicGroup
    {
        public MusicGroup(string name, IEnumerable<MusicUi> items)
        {
            Name = name;
            Items = items;
        }

        public string Name { get; }
        public IEnumerable<MusicUi> Items { get; }
    }
}
