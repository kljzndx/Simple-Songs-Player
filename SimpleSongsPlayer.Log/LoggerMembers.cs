﻿using Windows.Storage;
using NLog;

namespace SimpleSongsPlayer.Log
{
    public static class LoggerMembers
    {
        public static Logger PagesLogger { get; }

        static LoggerMembers()
        {
            LogManager.Configuration.Variables["localFolderPath"] = ApplicationData.Current.LocalFolder.Path;
            LogManager.Configuration.Variables["tempFolderPath"] = ApplicationData.Current.TemporaryFolder.Path;

            PagesLogger = LogManager.GetLogger("pages");
        }
        
    }
}