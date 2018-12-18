using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;

namespace SimpleSongsPlayer.ViewModels.Attributes
{
    public class PageTitleAttribute : Attribute
    {
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
            string str = ResourceLoader.GetForCurrentView(StringResourceName).GetString(StringResourceKey);
            if (String.IsNullOrWhiteSpace(str))
                throw new NullReferenceException("No found title");

            return str;
        }
    }
}