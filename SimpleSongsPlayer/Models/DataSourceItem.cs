using SimpleSongsPlayer.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.Models
{
    public class DataSourceItem
    {
        public DataSourceItem(string name, Func<MusicFileManageService, List<MusicUi>> queryAction, bool canRemove)
        {
            Name = name;
            QueryAction = queryAction;
            CanRemove = canRemove;
        }

        public string Name { get; set; }
        public Func<MusicFileManageService, List<MusicUi>> QueryAction { get; set; }
        public bool CanRemove { get; set; }
    }
}
