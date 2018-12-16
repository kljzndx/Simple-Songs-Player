using System;
using System.Reflection;
using System.Text.RegularExpressions;
using SimpleSongsPlayer.Models.DTO.Lyric.Attributes;

namespace SimpleSongsPlayer.Models.DTO.Lyric
{
    public class LyricProperties
    {
        [LyricsTag("ti")] private string title;
        [LyricsTag("ar")] private string artist;
        [LyricsTag("al")] private string album;
        [LyricsTag("by")] private string madeBy;
        [LyricsTag("re")] private string editorName;
        [LyricsTag("ve")] private string editorVersion;

        private LyricProperties()
        {
            title = String.Empty;
            artist = String.Empty;
            album = String.Empty;
            madeBy = String.Empty;
            editorName = String.Empty;
            editorVersion = String.Empty;
        }

        internal LyricProperties(string from) : this()
        {
            var typeInfo = GetType().GetTypeInfo();
            foreach (var fieldInfo in typeInfo.DeclaredFields)
            {
                var attribute = fieldInfo.GetCustomAttribute<LyricsTagAttribute>();
                if (attribute is null)
                    continue;

                Regex regex = new Regex($@"^\[{attribute.Name}\:(?<value>.*)\]");
                var match = regex.Match(from);
                if (match.Success)
                    fieldInfo.SetValue(this, match.Groups["value"].Value);
            }
        }

        public string Title => title;
        public string Artist => artist;
        public string Album => album;
        public string MadeBy => madeBy;
        public string EditorName => editorName;
        public string EditorVersion => editorVersion;
    }
}