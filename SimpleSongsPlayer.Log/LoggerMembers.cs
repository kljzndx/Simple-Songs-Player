using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using NLog;
using NLog.Config;

namespace SimpleSongsPlayer.Log
{
    public static class LoggerMembers
    {
        public static Logger PagesLogger { get; }

        static LoggerMembers()
        {
            LogManager.Configuration.Variables["tempFolderPath"] = ApplicationData.Current.TemporaryFolder.Path;
            PagesLogger = LogManager.GetLogger("pages");
        }
        
    }
}
