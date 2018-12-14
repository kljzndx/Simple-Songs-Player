using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace SimpleSongsPlayer.Models
{
    public delegate Task MusicItemMenuItemAction<TOption>(TOption source);

    public class MusicItemMenuItem<TOption>
    {
        private static readonly Dictionary<string, string> AllName = new Dictionary<string, string>();

        public MusicItemMenuItem(string resourceName, string resourceKey, MusicItemMenuItemAction<TOption> action)
        {
            string target = String.Format("{0}/{1}", resourceName, resourceKey);
            if (!AllName.ContainsKey(target))
                AllName[target] = ResourceLoader.GetForCurrentView(resourceName).GetString(resourceKey);

            Name = AllName[target];
            Action = action;
        }

        public string Name { get; }
        public MusicItemMenuItemAction<TOption> Action { get; }
    }
}