using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;

namespace SimpleSongsPlayer.ViewModels.Attributes
{
    public class SideViewNameAttribute : Attribute
    {
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
            return ResourceLoader.GetForCurrentView(TextResourceName).GetString(Key);
        }
    }
}