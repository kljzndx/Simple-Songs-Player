using SimpleSongsPlayer.Service.Models.Attributes;

namespace SimpleSongsPlayer.Service.Models
{
    public enum LoggerMembers
    {
        [LoggerInfo("\\logs\\ui.log", "\\logs\\UI")]
        App,
        [LoggerInfo("\\logs\\ui.log", "\\logs\\UI")]
        Ui,
        [LoggerInfo("\\logs\\background.log", "\\logs\\Background")]
        Service,
        [LoggerInfo("\\logs\\ui.log", "\\logs\\UI")]
        UnitTest
    }
}