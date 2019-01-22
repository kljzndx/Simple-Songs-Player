using System;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Log.Models;

namespace SimpleSongsPlayer.DAL.Factory
{
    public static class FilePropertiesGetter
    {
        static FilePropertiesGetter()
        {
            LogExtension.SetUpAssembly(typeof (FilePropertiesGetter).GetTypeInfo().Assembly, LoggerMembers.Other);
        }

        public static async Task<T> GetProperties<T>(this StorageFile file, Func<StorageFile, Task<T>> getter) where T : class
        {
            T result = null;
            int errorTimes = 0;

            do
            {
                try
                {
                    typeof(FilePropertiesGetter).LogByType("获取属性");
                    result = await getter.Invoke(file);
                }
                catch
                {
                    typeof(FilePropertiesGetter).LogByType("获取属性失败");
                    if (++errorTimes == 10)
                        throw;

                    typeof(FilePropertiesGetter).LogByType("将在1秒后重新获取");
                    await Task.Delay(1000);
                }
            } while (result is null);

            return result;
        }
    }
}