using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;

namespace SimpleSongsPlayer.ViewModels.SettingProperties
{
    public class ViewSettingProperties : SettingsBase
    {
        public static readonly ViewSettingProperties Current = new ViewSettingProperties();

        [SettingFieldByNormal(nameof(ScrollLyrics_FontSize), 15D)] private double scrollLyrics_FontSize;

        private ViewSettingProperties() : base("View")
        {
        }

        public double ScrollLyrics_FontSize
        {
            get => scrollLyrics_FontSize;
            set => SetSetting(ref scrollLyrics_FontSize, value);
        }
    }
}