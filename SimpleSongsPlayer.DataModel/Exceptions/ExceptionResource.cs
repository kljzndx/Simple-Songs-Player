using Windows.ApplicationModel.Resources;

namespace SimpleSongsPlayer.DataModel.Exceptions
{
    public static class ExceptionResource
    {
        public static ResourceLoader ErrorInfoStrings => ResourceLoader.GetForCurrentView("ErrorInfo");
    }
}