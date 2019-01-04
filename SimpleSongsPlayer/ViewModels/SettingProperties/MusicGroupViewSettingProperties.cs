﻿using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;

namespace SimpleSongsPlayer.ViewModels.SettingProperties
{

    public enum MusicGroupSorterMembers
    {
        Title,
        Count
    }

    public class MusicGroupViewSettingProperties : SettingsBase
    {
        public static readonly MusicGroupViewSettingProperties Current = new MusicGroupViewSettingProperties();

        private MusicGroupViewSettingProperties() : base("MusicGroupView")
        {
        }

        [SettingFieldByEnum(nameof(SortMethod), typeof(MusicGroupSorterMembers), nameof(MusicGroupSorterMembers.Title))] private MusicGroupSorterMembers sortMethod;

        public MusicGroupSorterMembers SortMethod
        {
            get => sortMethod;
            set => SetSetting(ref sortMethod, value);
        }

        [SettingFieldByNormal(nameof(IsReverse), false)] private bool isReverse;

        public bool IsReverse
        {
            get => isReverse;
            set => SetSetting(ref isReverse, value);
        }
    }
}