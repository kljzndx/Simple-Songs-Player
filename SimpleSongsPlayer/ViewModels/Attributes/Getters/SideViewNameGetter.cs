using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimpleSongsPlayer.ViewModels.Attributes.Getters
{
    public static class SideViewNameGetter
    {
        private static readonly object NameGetterLocker = new object();
        private static readonly Dictionary<string, string> AllNames = new Dictionary<string, string>();

        public static string GetNameFromType(Type type)
        {
            if (!AllNames.ContainsKey(type.FullName))
                lock (NameGetterLocker)
                    if (!AllNames.ContainsKey(type.FullName))
                    {
                        var attribute = type.GetTypeInfo().GetCustomAttribute<SideViewNameAttribute>();
                        if (attribute is null)
                            return String.Empty;
                        AllNames.Add(type.FullName, attribute.GetName());
                    }

            return AllNames[type.FullName];
        }
    }
}