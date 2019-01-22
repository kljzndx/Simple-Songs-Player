using System;

namespace SimpleSongsPlayer.Log.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class LoggerInfoAttribute : Attribute
    {
        public LoggerInfoAttribute(string nowPath, string oldPath, string name = null)
        {
            Name = name;
            NowPath = nowPath;
            OldPath = oldPath;
        }

        public string Name { get; set; }
        public string NowPath { get; }
        public string OldPath { get; }
    }
}