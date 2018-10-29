using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleSongsPlayer.Log
{
    public class LogContentGetter
    {
        private static StorageFile logFile;
        
        public static async Task<string> GetContent(uint lineCount)
        {
            if (logFile is null)
                logFile = await ApplicationData.Current.LocalFolder.GetFileAsync("logs\\main.log");

            var lines = (await FileIO.ReadLinesAsync(logFile)).ToList();
            lines.Reverse();

            var builder = new StringBuilder();
            int i = 0;
            foreach (var line in lines)
            {
                i++;
                if (i < lineCount)
                    builder.AppendLine(line);
                else
                    break;
            }
            return builder.ToString().Trim();
        }
    }
}