using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;

namespace SimpleSongsPlayer.ViewModels.SettingProperties
{
    public class OtherSettingProperties : SettingsBase
    {
        public static OtherSettingProperties Current = new OtherSettingProperties();

        private OtherSettingProperties() : base("Other")
        {
        }

        [SettingFieldByNormal(nameof(IsMigratedOldFavorites), false)] private bool isMigratedOldFavorites;

        public bool IsMigratedOldFavorites
        {
            get => isMigratedOldFavorites;
            set => SetSetting(ref isMigratedOldFavorites, value);
        }

        [SettingFieldByNormal(nameof(IsTimedExitEnable), false)] private bool isTimedExitEnable;

        public bool IsTimedExitEnable
        {
            get => isTimedExitEnable;
            set => SetSetting(ref isTimedExitEnable, value);
        }

        [SettingFieldByNormal(nameof(TimedExitMinutes), 15D)] private double timedExitMinutes;

        public double TimedExitMinutes
        {
            get => timedExitMinutes;
            set => SetSetting(ref timedExitMinutes, value);
        }
    }
}