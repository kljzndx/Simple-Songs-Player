using SimpleSongsPlayer.Log.Models.Attributes;

namespace SimpleSongsPlayer.Log.Models
{
    public enum LoggerMembers
    {
        [LoggerInfo("\\logs\\main.log", "\\logs\\Main")]
        App,
        [LoggerInfo("\\logs\\main.log", "\\logs\\Main")]
        Ui,
        [LoggerInfo("\\logs\\main.log", "\\logs\\Main")]
        Service,
        [LoggerInfo("\\logs\\main.log", "\\logs\\Main")]
        UnitTest,
        [LoggerInfo("\\logs\\main.log", "\\logs\\Main")]
        Other
    }
}