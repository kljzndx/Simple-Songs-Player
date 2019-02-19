using System;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace SimpleSongsPlayer.ViewModels.Getters
{
    public static class PrivateKeyGetter
    {
        private const string ADBIG = "Ad Big";
        private const string ADSMALL = "Ad Small";
        private const string REMOVEADS = "Remove Ads Addition";

        private static bool _isInit;

        public static string AdBigKey { get; private set; }
        public static string AdSmallKey { get; private set; }
        public static string RemoveAdKey { get; private set; }
        
        public static async Task Init()
        {
            if (_isInit)
                return;

            _isInit = true;
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/PrivateKey.json"));
            string json = await FileIO.ReadTextAsync(file);
            JsonArray jsonArray = JsonArray.Parse(json);
            foreach (var item in jsonArray)
            {
                var jobj = item.GetObject();
                switch (jobj["Name"].GetString())
                {
                    case ADBIG:
                        AdBigKey = jobj["Key"].GetString();
                        break;
                    case ADSMALL:
                        AdSmallKey = jobj["Key"].GetString();
                        break;
                    case REMOVEADS:
                        RemoveAdKey = jobj["Key"].GetString();
                        break;
                }
            }
        }

    }
}