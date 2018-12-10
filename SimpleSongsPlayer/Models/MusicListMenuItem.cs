using System;

namespace SimpleSongsPlayer.Models
{
    public class MusicListMenuItem
    {
        public MusicListMenuItem(string resourceKey, Action<MusicFileDynamic> action)
        {
            Name = StringResources.ListStringResource.GetString(resourceKey);
            Action = action;
        }

        public string Name { get; }
        public Action<MusicFileDynamic> Action { get; }
    }
}