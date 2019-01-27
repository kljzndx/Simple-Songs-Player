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
        private static readonly ResourceLoader StringResource = ResourceLoader.GetForCurrentView("MoreMenu");

        public MusicItemMenuItem(string resourceKey, MusicItemMenuItemAction<TOption> action)
        {
            string target = resourceKey;
            if (!AllName.ContainsKey(target))
                AllName[target] = StringResource.GetString(resourceKey);

            Name = AllName[target];
            Action = action;
        }

        public string Name { get; }
        public MusicItemMenuItemAction<TOption> Action { get; }
    }
}