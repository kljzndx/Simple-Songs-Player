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
    }
}