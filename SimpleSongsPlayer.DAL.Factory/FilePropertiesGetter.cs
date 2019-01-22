using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleSongsPlayer.DAL.Factory
{
    public static class FilePropertiesGetter
    {
        public static async Task<T> GetProperties<T>(this StorageFile file, Func<StorageFile, Task<T>> getter) where T : class
        {
            T result = null;
            int errorTimes = 0;

            do
            {
                try
                {
                    result = await getter.Invoke(file);
                }
                catch
                {
                    if (++errorTimes == 10)
                        throw;

                    await Task.Delay(1000);
                }
            } while (result is null);

            return result;
        }
    }
}