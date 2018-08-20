using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleSongsPlayer.DataModel
{
    public class LyricBlock
    {
        private static Regex lyricLineRegex = new Regex(@"\[(?<min>\d{2})\:(?<ss>\d{2}).(?<ms>\d{1,3})\](?<content>.*)");

        /// <summary>
        /// 通过文本创建歌词
        /// </summary>
        /// <param name="content">文本内容</param>
        public LyricBlock(string content)
        {
            Properties = new LyricsProperties(content);
            Lines = new List<LyricLine>();

            string[] strLines = content.Contains("\n") ? content.Split('\n') : content.Split('\r');

            StringBuilder builder = new StringBuilder();
            LyricLine currentLine = null;

            foreach (string item in strLines)
            {
                if (String.IsNullOrWhiteSpace(item))
                    continue;
                string str = item.Trim();

                var match = lyricLineRegex.Match(item);
                if (match.Success)
                {
                    int min = Int32.Parse(match.Groups["min"].Value);
                    int ss = Int32.Parse(match.Groups["ss"].Value);
                    string msbit = "000";
                    string msStr = match.Groups["ms"].Value;
                    int ms = Int32.Parse(msStr + msbit.Remove(2 - msStr.Length));

                    if (currentLine != null)
                    {
                        currentLine.Content = builder.ToString().Trim();
                        Lines.Add(currentLine);
                        builder.Clear();
                    }

                    currentLine = new LyricLine(new TimeSpan(0, 0, min, ss, ms));
                }
                else
                {
                    builder.AppendLine(str);
                }
            }

            if (currentLine != null)
                Lines.Add(currentLine);

            Lines.Sort();
        }

        public LyricsProperties Properties { get; }
        public List<LyricLine> Lines { get; }
    }
}