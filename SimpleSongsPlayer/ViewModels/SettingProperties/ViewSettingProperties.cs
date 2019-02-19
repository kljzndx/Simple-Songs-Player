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

        [SettingFieldByNormal(nameof(BackgroundBlurDegree), 10D)] private double backgroundBlurDegree;

        public double BackgroundBlurDegree
        {
            get => backgroundBlurDegree;
            set => SetSetting(ref backgroundBlurDegree, value);
        }

        [SettingFieldByNormal(nameof(BackgroundTransparency), 0.65D)] private double backgroundTransparency;

        public double BackgroundTransparency
        {
            get => backgroundTransparency;
            set => SetSetting(ref backgroundTransparency, value);
        }

        [SettingFieldByNormal(nameof(IsShowAds), true)] private bool isShowAds;

        public bool IsShowAds
        {
            get => isShowAds;
            set => SetSetting(ref isShowAds, value);
        }
    }
}