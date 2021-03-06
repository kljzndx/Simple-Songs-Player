﻿using HappyStudio.UwpToolsLibrary.Auxiliarys;
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

        private bool canOptionNowPlayList = true;

        public bool CanOptionNowPlayList
        {
            get => canOptionNowPlayList;
            set => Set(ref canOptionNowPlayList, value);
        }

        [SettingFieldByNormal(nameof(CurrentPlayIndex), (uint) 0)] private uint currentPlayIndex;

        public uint CurrentPlayIndex
        {
            get => currentPlayIndex;
            set => SetSetting(ref currentPlayIndex, value);
        }

        [SettingFieldByNormal(nameof(ClassifyViewId), 0)] private int _classifyViewId;

        public int ClassifyViewId
        {
            get => _classifyViewId;
            set => SetSetting(ref _classifyViewId, value);
        }
    }
}