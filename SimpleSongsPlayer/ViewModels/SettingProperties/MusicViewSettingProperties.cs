using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;

namespace SimpleSongsPlayer.ViewModels.SettingProperties
{
    public enum MusicGrouperMembers
    {
        None,
        FirstLetter
    }

    public enum MusicSorterMembers
    {
        Title,
        Artist,
        Album,
        ChangeDate
    }

    public class MusicViewSettingProperties : SettingsBase
    {
        public static MusicViewSettingProperties Current = new MusicViewSettingProperties();

        [SettingFieldByEnum(nameof(GroupMethod), typeof(MusicGrouperMembers), nameof(MusicGrouperMembers.None))] private MusicGrouperMembers groupMethod;
        [SettingFieldByEnum(nameof(SortMethod), typeof(MusicSorterMembers), nameof(MusicSorterMembers.ChangeDate))] private MusicSorterMembers sortMethod;
        [SettingFieldByNormal(nameof(IsReverse), true)] private bool isReverse;

        private MusicViewSettingProperties() : base("MusicView")
        {
        }

        public MusicGrouperMembers GroupMethod
        {
            get => groupMethod;
            set => SetSetting(ref groupMethod, value, settingValue: value.ToString());
        }

        public MusicSorterMembers SortMethod
        {
            get => sortMethod;
            set => SetSetting(ref sortMethod, value, settingValue: value.ToString());
        }

        public bool IsReverse
        {
            get => isReverse;
            set => SetSetting(ref isReverse, value);
        }
    }
}