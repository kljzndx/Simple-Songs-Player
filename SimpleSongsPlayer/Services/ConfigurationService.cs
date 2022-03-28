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
        None,
        Title,
        TrackNumber,
        ModifyDate
    }

    public enum MusicGroupListSortEnum
    {
        Name,
        Count
    }

    public enum MusicListGroupMethodEnum
    {
        None,
        FirstCharacterOfTitle,
        Artist,
        Album,
        Folder
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

        [SettingFieldByNormal(nameof(DataSourceId), 0)]
        private int _dataSourceId;
        [SettingFieldByEnum(nameof(MusicListGroupMethod), typeof(MusicListGroupMethodEnum), "FirstCharacterOfTitle")]
        private MusicListGroupMethodEnum _musicListGroupMethod;
        [SettingFieldByEnum(nameof(MusicListSort), typeof(MusicListSortEnum), "Title")]
        private MusicListSortEnum _musicListSort;
        [SettingFieldByEnum(nameof(MusicGroupListSort), typeof(MusicGroupListSortEnum), "Name")]
        private MusicGroupListSortEnum _musicGroupListSort;
        [SettingFieldByNormal(nameof(IsReverseMusicGroupList), false)]
        private bool _isReverseMusicGroupList;
        [SettingFieldByNormal(nameof(IsReverseMusicList), false)]
        private bool _isReverseMusicList;

        [SettingFieldByNormal(nameof(LyricPreviewFontSize), 14D)]
        private double _lyricPreviewFontSize;
        [SettingFieldByNormal(nameof(LyricPreviewOpacity), 0.4)]
        private double _lyricPreviewOpacity;

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

        public int DataSourceId
        {
            get => _dataSourceId;
            set => SetSetting(ref _dataSourceId, value);
        }
        public MusicListGroupMethodEnum MusicListGroupMethod
        {
            get => _musicListGroupMethod;
            set => SetSetting(ref _musicListGroupMethod, value);
        }
        public MusicGroupListSortEnum MusicGroupListSort
        {
            get => _musicGroupListSort;
            set => SetSetting(ref _musicGroupListSort, value);
        }
        public MusicListSortEnum MusicListSort
        {
            get => _musicListSort;
            set => SetSetting(ref _musicListSort, value);
        }
        public bool IsReverseMusicGroupList
        {
            get => _isReverseMusicGroupList;
            set => SetSetting(ref _isReverseMusicGroupList, value);
        }
        public bool IsReverseMusicList
        {
            get => _isReverseMusicList;
            set => SetSetting(ref _isReverseMusicList, value);
        }

        public double LyricPreviewFontSize
        {
            get => _lyricPreviewFontSize;
            set => SetSetting(ref _lyricPreviewFontSize, value);
        }
        public double LyricPreviewOpacity
        {
            get => _lyricPreviewOpacity;
            set => SetSetting(ref _lyricPreviewOpacity, value);
        }
    }
}
