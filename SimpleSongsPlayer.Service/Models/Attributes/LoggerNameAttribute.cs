using System;

namespace SimpleSongsPlayer.Service.Models.Attributes
{
    public class LoggerNameAttribute : Attribute
    {
        public LoggerNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}