using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;
using SimpleSongsPlayer.Models;

namespace SimpleSongsPlayer.ViewModels
{
    public class SettingProperties : SettingsBase
    {
        public static readonly SettingProperties Current = new SettingProperties();

        [SettingFieldByNormal(nameof(Volume), 1D)] private double volume;
        [SettingFieldByNormal(nameof(PlaybackSpeed), 1D)] private double playbackSpeed;
        [SettingFieldByNormal(nameof(ScrollLyrics_FontSize), 15D)] private double scrollLyrics_FontSize;

        private SettingProperties()
        {
            InitializeSettingFields();
        }

        public double Volume
        {
            get => volume;
            set => SetSetting(ref volume, value);
        }

        public double PlaybackSpeed
        {
            get => playbackSpeed;
            set => SetSetting(ref playbackSpeed, value);
        }

        public double ScrollLyrics_FontSize
        {
            get => scrollLyrics_FontSize;
            set => SetSetting(ref scrollLyrics_FontSize, value);
        }

        [SettingFieldByNormal(nameof(IsTimerPauseEnable), false)] private bool isTimerPauseEnable;

        public bool IsTimerPauseEnable
        {
            get => isTimerPauseEnable;
            set => SetSetting(ref isTimerPauseEnable, value);
        }

        [SettingFieldByNormal(nameof(PauseTimeMinutes), 10D)] private double pauseTimeMinutes;

        public double PauseTimeMinutes
        {
            get => pauseTimeMinutes;
            set => SetSetting(ref pauseTimeMinutes, value);
        }

        [SettingFieldByEnum(nameof(LoopingMode), typeof(LoopingModeEnum), nameof(LoopingModeEnum.List))]
        private LoopingModeEnum loopingMode;

        public LoopingModeEnum LoopingMode
        {
            get => loopingMode;
            set => SetSetting(ref loopingMode, value);
        }
    }
}