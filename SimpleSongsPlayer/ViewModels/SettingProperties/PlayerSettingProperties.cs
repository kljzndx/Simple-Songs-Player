using Windows.Storage;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;

namespace SimpleSongsPlayer.ViewModels.SettingProperties
{
    public class PlayerSettingProperties : SettingsBase
    {
        public static readonly PlayerSettingProperties Current = new PlayerSettingProperties();

        [SettingFieldByEnum(nameof(RepeatMode), typeof(PlaybackRepeatModeEnum), nameof(PlaybackRepeatModeEnum.List))] private PlaybackRepeatModeEnum repeatMode;
        [SettingFieldByNormal(nameof(Volume), 1D)] private double volume;

        private PlayerSettingProperties() : base(ApplicationData.Current.LocalSettings.CreateContainer("Player", ApplicationDataCreateDisposition.Always))
        {
        }

        public PlaybackRepeatModeEnum RepeatMode
        {
            get => repeatMode;
            set => SetSetting(ref repeatMode, value, settingValue: value.ToString());
        }

        public double Volume
        {
            get => volume;
            set => SetSetting(ref volume, value);
        }
    }
}