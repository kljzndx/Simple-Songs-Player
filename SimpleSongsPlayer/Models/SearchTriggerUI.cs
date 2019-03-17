using System;

namespace SimpleSongsPlayer.Models
{
    public class SearchTriggerUI<T>
    {
        public SearchTriggerUI(string resourceKey, Func<T, string> selector)
        {
            Name = StringResources.SearchTriggerMembersStringResource.GetString(resourceKey);
            Selector = selector;
        }

        public string Name { get; }
        public Func<T, string> Selector { get; }
    }
}