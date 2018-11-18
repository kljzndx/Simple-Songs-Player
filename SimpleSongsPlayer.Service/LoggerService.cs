using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using NLog;
using SimpleSongsPlayer.Service.Models;
using SimpleSongsPlayer.Service.Models.Attributes;

namespace SimpleSongsPlayer.Service
{
    public static class LoggerService
    {
        private static readonly object LoggerGetting_Locker = new object();

        private static readonly Dictionary<LoggerMembers, KeyValuePair<LoggerInfoAttribute, Logger>> AllLoggers = new Dictionary<LoggerMembers, KeyValuePair<LoggerInfoAttribute, Logger>>();
        private static readonly List<FieldInfo> MembersInfo;
        private static readonly Regex LogRegex = new Regex(@"^(\d{4}\/\d{1,2}\/\d{1,2}).*\|");

        static LoggerService()
        {
            MembersInfo = typeof(LoggerMembers).GetTypeInfo().DeclaredFields.ToList();

            LogManager.Configuration.Variables["localFolderPath"] = ApplicationData.Current.LocalFolder.Path;
            LogManager.Configuration.Variables["tempFolderPath"] = ApplicationData.Current.TemporaryFolder.Path;
        }

        public static Logger GetLogger(LoggerMembers member)
        {
            Initialize(member);

            return AllLoggers[member].Value;
        }

        public static async Task<string> ReadLogs(LoggerMembers member, int lineCount)
        {
            Initialize(member);
            var file = await StorageFile.GetFileFromPathAsync(String.Format("{0}{1}", ApplicationData.Current.LocalFolder.Path, AllLoggers[member].Key.NowPath));
            var lines = await FileIO.ReadLinesAsync(file);

            var builder = new StringBuilder();
            int hasLines = 0;

            for (var i = lines.Count - 1; i >= 0 && hasLines < lineCount; i--)
            {
                if (LogRegex.IsMatch(lines[i]))
                    hasLines++;

                builder.AppendLine(lines[i]);
            }

            return builder.ToString().Trim();
        }

        private static void Initialize(LoggerMembers member)
        {
            if (!AllLoggers.ContainsKey(member))
                lock (LoggerGetting_Locker)
                    if (!AllLoggers.ContainsKey(member))
                    {
                        var info = GetName(member);
                        AllLoggers.Add(member,
                            new KeyValuePair<LoggerInfoAttribute, Logger>(info, LogManager.GetLogger(info.Name)));
                    }
        }

        private static LoggerInfoAttribute GetName(LoggerMembers member)
        {
            var info = MembersInfo.First(i => i.Name == member.ToString());
            var result = info.GetCustomAttribute<LoggerInfoAttribute>();
            if (result.Name is null)
            {
                string firstWord = info.Name.First().ToString();
                var restWords = info.Name.Remove(0, 1);
                result.Name = String.Format("{0}{1}", firstWord.ToLower(), restWords);
            }
            return result;
        }
    }
}