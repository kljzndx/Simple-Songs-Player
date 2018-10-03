using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using SimpleSongsPlayer.DataModel.Exceptions;

namespace SimpleSongsPlayer.DataModel
{
    public class LyricBlock
    {
        private static readonly Regex LyricLineRegex = new Regex(@"^\[(?<min>\d+)\:(?<ss>\d{2}).(?<ms>\d{1,3})\](?<content>.*)");

        /// <summary>
        /// 通过文本创建歌词
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="content">文件内容</param>
        public LyricBlock(string fileName, string content)
        {
            FileName = fileName;
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

                var match = LyricLineRegex.Match(str);
                if (match.Success)
                {
                    try
                    {
                        int min = Int32.Parse(match.Groups["min"].Value);
                        int ss = Int32.Parse(match.Groups["ss"].Value);
                        string msStr = match.Groups["ms"].Value;

                        for (int i = msStr.Length; i < 3; i++)
                            msStr += "0";

                        int ms = Int32.Parse(msStr);

                        if (currentLine != null)
                        {
                            currentLine.Content = builder.ToString().Trim();
                            Lines.Add(currentLine);
                            builder.Clear();
                        }

                        currentLine = new LyricLine(new TimeSpan(0, 0, min, ss, ms));
                        builder.AppendLine(match.Groups["content"].Value.Trim());
                    }
                    catch (Exception)
                    {
                        throw new LyricsParsingFailedException(fileName);
                    }
                }
                else if (currentLine != null)
                {
                    builder.AppendLine(str);
                }
            }

            if (currentLine != null)
                Lines.Add(currentLine);

            Lines.Sort();
        }

        public string FileName { get; }
        public LyricsProperties Properties { get; }
        public List<LyricLine> Lines { get; }
    }
}