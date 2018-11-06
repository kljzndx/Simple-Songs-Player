using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;

namespace SimpleSongsPlayer.ViewModels.Attributes
{
    public class SideViewNameAttribute : Attribute
    {
        private static readonly object TextGetLocker = new object();
        private static readonly Dictionary<string, string> NamesCache = new Dictionary<string, string>();

        public SideViewNameAttribute(string textResourceName) : this(textResourceName, "Title")
        {
        }

        public SideViewNameAttribute(string textResourceName, string key)
        {
            TextResourceName = textResourceName;
            Key = key;
        }

        public string TextResourceName { get; }
        public string Key { get; }

        public string GetName()
        {
            if (!NamesCache.ContainsKey(TextResourceName))
                lock (TextGetLocker)
                    if (!NamesCache.ContainsKey(TextResourceName))
                        NamesCache.Add(TextResourceName, ResourceLoader.GetForCurrentView(TextResourceName).GetString(Key));

            return NamesCache[TextResourceName];
        }
    }
}