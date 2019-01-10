using GalaSoft.MvvmLight;
using SimpleSongsPlayer.DAL;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleSongsPlayer.Models.DTO.Lyric
{
    public class LyricFileDTO : ObservableObject
    {
        private static readonly Encoding GbkEncoding = GetGbkEncoding();
        private static readonly Regex LyricLineRegex = new Regex(@"^\[(?<min>\d+)\:(?<ss>\d{2}).(?<ms>\d{1,3})\](?<content>.*)");

        public static LyricFileDTO Empty { get; } = new LyricFileDTO { IsInit = true };

        private LyricProperties properties;
        private List<LyricLine> lines;

        private LyricFileDTO()
        {
        }

        public LyricFileDTO(LyricFile file)
        {
            FileName = file.FileName;
            FilePath = file.Path;
            ModifyDate = file.ChangeDate;
        }

        public bool IsInit { get; private set; }

        public string FileName { get; }
        public string FilePath { get; }
        public DateTime ModifyDate { get; private set; }

        public LyricProperties Properties
        {
            get => properties;
            private set => Set(ref properties, value);
        }

        public List<LyricLine> Lines
        {
            get => lines;
            private set => Set(ref lines, value);
        }

        public async Task Update(LyricFile file)
        {
            if (IsInit && file.Path == FilePath && file.ChangeDate != ModifyDate)
            {
                IsInit = false;
                ModifyDate = file.ChangeDate;
                await Init();
            }
        }

        public async Task Init()
        {
            if (IsInit)
                return;

            IsInit = true;

            StorageFile file = await StorageFile.GetFileFromPathAsync(FilePath);
            string content = await ReadText(file);

            Properties = new LyricProperties(content);
            var result = new List<LyricLine>();

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
                        int min = Int32.Parse(ToDBC(match.Groups["min"].Value));
                        int ss = Int32.Parse(ToDBC(match.Groups["ss"].Value));
                        string msStr = ToDBC(match.Groups["ms"].Value);

                        for (int i = msStr.Length; i < 3; i++)
                            msStr += "0";

                        int ms = Int32.Parse(msStr);

                        if (currentLine != null)
                        {
                            currentLine.Content = builder.ToString().Trim();
                            result.Add(currentLine);
                            builder.Clear();
                        }

                        currentLine = new LyricLine(new TimeSpan(0, 0, min, ss, ms));
                        builder.AppendLine(match.Groups["content"].Value.Trim());
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Unable to resolve file {file.DisplayName}", e);
                    }
                }
                else if (currentLine != null)
                {
                    builder.AppendLine(str);
                }
            }

            if (currentLine != null)
                result.Add(currentLine);

            result.Sort();

            Lines = result;
        }

        private async Task<string> ReadText(IStorageFile file)
        {
            string result;

            try
            {
                result = await FileIO.ReadTextAsync(file);
            }
            catch (Exception)
            {
                var buffer = await FileIO.ReadBufferAsync(file);
                var bytes = buffer.ToArray();

                result = GbkEncoding.GetString(bytes);
            }

            return result;
        }

        /// <summary>
        /// 半角转全角
        /// </summary>
        /// <param name="input">要转换的字符串</param>
        /// <returns>全角字符串</returns>
        public static string ToSBC(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }

                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new String(c);
        }
        /// <summary>
        /// 全角转半角
        /// </summary>
        /// <param name="input">要转换的字符串</param>
        /// <returns>半角字符串</returns>
        public static string ToDBC(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }

                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new String(c);
        }

        
        private static Encoding GetGbkEncoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding("GBK");
        }
    }
}