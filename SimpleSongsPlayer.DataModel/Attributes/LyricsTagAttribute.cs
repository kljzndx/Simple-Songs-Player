using System;

namespace SimpleSongsPlayer.DataModel.Attributes
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