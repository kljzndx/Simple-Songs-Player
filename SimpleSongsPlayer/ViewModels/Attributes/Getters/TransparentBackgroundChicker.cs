using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimpleSongsPlayer.ViewModels.Attributes.Getters
{
    public static class TransparentBackgroundChecker
    {
        private static readonly Dictionary<Type, bool> AllCheckResult = new Dictionary<Type, bool>();

        public static bool Check(Type pageType)
        {
            if (!AllCheckResult.ContainsKey(pageType))
            {
                var attribute = pageType.GetTypeInfo().GetCustomAttribute<TransparentBackgroundAttribute>();
                AllCheckResult[pageType] = attribute != null;
            }

            return AllCheckResult[pageType];
        }
    }
}