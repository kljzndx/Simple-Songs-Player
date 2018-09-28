using System;

namespace SimpleSongsPlayer.DataModel.Exceptions
{
    public class LyricsParsingFailedException : Exception
    {
        public LyricsParsingFailedException(string fileName) : base($"\"{fileName}\" {ExceptionResource.ErrorInfoStrings.GetString("LyricsParseFail")}")
        {
        }
    }
}