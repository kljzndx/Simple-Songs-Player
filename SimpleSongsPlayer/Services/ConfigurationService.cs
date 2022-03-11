using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.Services
{
    public enum MusicListSortEnum
    {
        Title,
        TrackNumber,
        ModifyDate
    }

    public enum MusicGroupSortEnum
    {
        Name,
        Count
    }

    public enum LoopingModeEnum
    {
        Single,
        List,
        Random
    }

    public class ConfigurationService : SettingsBase
    {
        [SettingFieldByNormal(nameof(Volume), 0.6D)]
        private double _volume;
        [SettingFieldByNormal(nameof(PlaybackRate), 1.0D)]
        private double _playbackRate;
        [SettingFieldByEnum(nameof(LoopingMode), typeof(LoopingModeEnum), "List")]
        private LoopingModeEnum _loopingMode;

        [SettingFieldByEnum(nameof(MusicListSort), typeof(MusicListSortEnum), "Title")]
        private MusicListSortEnum _musicListSort;
        [SettingFieldByEnum(nameof(MusicGroupSort), typeof(MusicGroupSortEnum), "Name")]
        private MusicGroupSortEnum _musicGroupSort;

        public double Volume
        {
            get => _volume;
            set => SetSetting(ref _volume, value);
        }
        public double PlaybackRate
        {
            get => _playbackRate;
            set => SetSetting(ref _playbackRate, value);
        }
        public LoopingModeEnum LoopingMode
        {
            get => _loopingMode;
            set => SetSetting(ref _loopingMode, value);
        }

        public MusicListSortEnum MusicListSort
        {
            get => _musicListSort;
            set => SetSetting(ref _musicListSort, value);
        }
        public MusicGroupSortEnum MusicGroupSort
        {
            get => _musicGroupSort;
            set => SetSetting(ref _musicGroupSort, value);
        }
    }
}
