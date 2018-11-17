using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.Storage;
using NLog;
using SimpleSongsPlayer.Service.Models;
using SimpleSongsPlayer.Service.Models.Attributes;

namespace SimpleSongsPlayer.Service
{
    public static class LoggerService
    {
        private static readonly object LoggerGetting_Locker = new object();

        private static readonly Dictionary<LoggerMembers, Logger> AllLoggers = new Dictionary<LoggerMembers, Logger>();
        private static readonly List<FieldInfo> MembersInfo;

        static LoggerService()
        {
            MembersInfo = typeof(LoggerMembers).GetTypeInfo().DeclaredFields.ToList();

            LogManager.Configuration.Variables["localFolderPath"] = ApplicationData.Current.LocalFolder.Path;
            LogManager.Configuration.Variables["tempFolderPath"] = ApplicationData.Current.TemporaryFolder.Path;
        }

        public static Logger GetLogger(LoggerMembers member)
        {
            if (!AllLoggers.ContainsKey(member))
                lock (LoggerGetting_Locker)
                    if (!AllLoggers.ContainsKey(member))
                        AllLoggers.Add(member, LogManager.GetLogger(GetName(member)));

            return AllLoggers[member];
        }

        private static string GetName(LoggerMembers member)
        {
            string result = String.Empty;

            var info = MembersInfo.First(i => i.Name == member.ToString());
            if (info.GetCustomAttribute<LoggerNameAttribute>() is LoggerNameAttribute loggerName)
                result = loggerName.Name;
            else
            {
                string firstWord = info.Name.First().ToString();
                var restWords = info.Name.Remove(0, 1);
                result = String.Format("{0}{1}", firstWord.ToLower(), restWords);
            }

            return result;
        }
    }
}