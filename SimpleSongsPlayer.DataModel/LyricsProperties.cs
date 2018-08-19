using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using SimpleSongsPlayer.DataModel.Attributes;

namespace SimpleSongsPlayer.DataModel
{
    public class LyricsProperties
    {
        private LyricsProperties()
        {
            Title = String.Empty;
            Aretist = String.Empty;
            Album = String.Empty;
            MadeBy = String.Empty;
            EditorName = String.Empty;
            EditorVersion = String.Empty;
        }

        internal LyricsProperties(string from) : this()
        {
            var typeInfo = GetType().GetTypeInfo();
            foreach (var propertyInfo in typeInfo.DeclaredProperties)
            {
                var attribute = propertyInfo.GetCustomAttribute<LyricsTagAttribute>();
                if (attribute is null)
                    continue;

                Regex regex = new Regex($@"\[{attribute.Name}:(?<value>.*)\]");
                var match = regex.Match(from);
                if (match.Success)
                    propertyInfo.SetValue(this, match.Groups["value"].Value);
            }
        }

        [LyricsTag("ti")] public string Title { get; }
        [LyricsTag("ar")] public string Aretist { get; }
        [LyricsTag("al")] public string Album { get; }
        [LyricsTag("by")] public string MadeBy { get; }
        [LyricsTag("re")] public string EditorName { get; }
        [LyricsTag("ve")] public string EditorVersion { get; }
    }
}