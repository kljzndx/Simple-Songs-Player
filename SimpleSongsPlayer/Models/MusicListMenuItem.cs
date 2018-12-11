using System;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.Models
{
    public delegate Task MusicListMenuItemAction(MusicFileDynamic source);

    public class MusicListMenuItem
    {
        public MusicListMenuItem(string resourceKey, MusicListMenuItemAction action)
        {
            Name = StringResources.ListStringResource.GetString(resourceKey);
            Action = action;
        }

        public string Name { get; }
        public MusicListMenuItemAction Action { get; }
    }
}