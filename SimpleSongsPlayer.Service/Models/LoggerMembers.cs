using SimpleSongsPlayer.Service.Models.Attributes;

namespace SimpleSongsPlayer.Service.Models
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
        UnitTest
    }
}