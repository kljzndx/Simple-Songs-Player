using System;

namespace SimpleSongsPlayer.Models.DTO.Lyric.Attributes
{
    public class LyricsTagAttribute : Attribute
    {
        public LyricsTagAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}