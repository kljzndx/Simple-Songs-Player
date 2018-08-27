using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleSongsPlayer.Operator
{
    public static class FileReader
    {
        private static readonly Encoding gbkEncoding;

        static FileReader()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            gbkEncoding = Encoding.GetEncoding("GBK");
        }

        public static async Task<string> ReadFileAsync(StorageFile file)
        {
            string result = String.Empty;

            try
            {
                result = await FileIO.ReadTextAsync(file);
            }
            catch (ArgumentOutOfRangeException)
            {
                var buffer = await FileIO.ReadBufferAsync(file);
                result = gbkEncoding.GetString(buffer.ToArray());
            }

            return result;
        }
    }
}