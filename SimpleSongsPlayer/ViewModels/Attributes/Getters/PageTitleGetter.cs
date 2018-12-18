using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.UI.Xaml.Controls;

namespace SimpleSongsPlayer.ViewModels.Attributes.Getters
{
    public static class PageTitleGetter
    {
        private static readonly Dictionary<string, string> AllTitle = new Dictionary<string, string>();

        public static string GetTitle(Type pageType)
        {
            if (!AllTitle.ContainsKey(pageType.FullName))
            {
                var attribute = pageType.GetTypeInfo().GetCustomAttribute<PageTitleAttribute>();
                if (attribute is null)
                    throw new NullReferenceException("No found title tag");
                string title = attribute.GetTitle();

                AllTitle.Add(pageType.FullName, title);
            }

            return AllTitle[pageType.FullName];
        }

        public static string TryGetTitle(Type pageTitle)
        {
            try
            {
                return GetTitle(pageTitle);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }
    }
}