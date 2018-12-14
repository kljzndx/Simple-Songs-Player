using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.ViewModels.Factories;

namespace SimpleSongsPlayer.ViewModels.Arguments
{
    [Flags]
    public enum MusicGroupArgsType
    {
        ItemSource = 1,
        GroupSource = 2,
        Grouper = 4,
        Menu = 8
    }

    public class MusicGroupArguments
    {
        public MusicGroupArguments(ObservableCollection<MusicFileGroup> source)
        {
            GroupSource = source;
            ArgsType = MusicGroupArgsType.GroupSource;
        }

        public MusicGroupArguments(ObservableCollection<MusicFileGroup> source, IEnumerable<MusicItemMenuItem<MusicFileGroupDynamic>> extraGroupMenu, IEnumerable<MusicItemMenuItem<MusicFileDynamic>> extraItemMenu)
        {
            GroupSource = source;
            ExtraGroupMenu = extraGroupMenu.ToList();
            ExtraItemMenu = extraItemMenu.ToList();
            ArgsType = MusicGroupArgsType.GroupSource | MusicGroupArgsType.Menu;
        }

        public MusicGroupArguments(ObservableCollection<MusicFileDTO> source, MusicGrouperArgs grouperArgs)
        {
            ItemSource = source;
            GrouperArgs = grouperArgs;
            ArgsType = MusicGroupArgsType.ItemSource | MusicGroupArgsType.Grouper;
        }

        public MusicGroupArguments(ObservableCollection<MusicFileDTO> source, MusicGrouperArgs grouperArgs, IEnumerable<MusicItemMenuItem<MusicFileGroupDynamic>> extraGroupMenu, IEnumerable<MusicItemMenuItem<MusicFileDynamic>> extraItemMenu)
        {
            ItemSource = source;
            GrouperArgs = grouperArgs;
            ExtraGroupMenu = extraGroupMenu.ToList();
            ExtraItemMenu = extraItemMenu.ToList();
            ArgsType = MusicGroupArgsType.ItemSource | MusicGroupArgsType.Grouper | MusicGroupArgsType.Menu;
        }

        public MusicGroupArgsType ArgsType { get; }
        public ObservableCollection<MusicFileDTO> ItemSource { get; }
        public ObservableCollection<MusicFileGroup> GroupSource { get; }
        public MusicGrouperArgs GrouperArgs { get; }
        public List<MusicItemMenuItem<MusicFileGroupDynamic>> ExtraGroupMenu { get; }
        public List<MusicItemMenuItem<MusicFileDynamic>> ExtraItemMenu { get; }
    }
}