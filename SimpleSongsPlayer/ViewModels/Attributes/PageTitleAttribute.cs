using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;

namespace SimpleSongsPlayer.ViewModels.Attributes
{
    public class PageTitleAttribute : Attribute
    {
        private static readonly Dictionary<string, string> AllTitles = new Dictionary<string, string>();

        public PageTitleAttribute(string stringResourceName, string stringResourceKey)
        {
            StringResourceName = stringResourceName;
            StringResourceKey = stringResourceKey;
        }

        public PageTitleAttribute(string stringResourceKey) : this("PageTitles", stringResourceKey)
        {
        }

        public string StringResourceName { get; }
        public string StringResourceKey { get; }

        public string GetTitle()
        {
            var key = String.Format("{0}/{1}", StringResourceName, StringResourceKey);

            if (!AllTitles.ContainsKey(key))
            {
                string str = ResourceLoader.GetForCurrentView(StringResourceName).GetString(StringResourceKey);
                if (String.IsNullOrWhiteSpace(str))
                    throw new NullReferenceException("No found title");

                AllTitles.Add(key, str);
            }

            return AllTitles[key];
        }
    }
}