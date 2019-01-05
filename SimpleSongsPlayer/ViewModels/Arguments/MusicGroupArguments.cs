using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.ViewModels.DataServers;
using SimpleSongsPlayer.ViewModels.Factories;

namespace SimpleSongsPlayer.ViewModels.Arguments
{
    [Flags]
    public enum MusicGroupArgsType
    {
        ItemSource = 1,
        GroupSource = 2,
        Grouper = 4,
        GroupMenu = 8,
        ItemMenu = 16,
        DataServer = 32
    }

    public class MusicGroupArguments : PageArgumentsBase
    {
        private ObservableCollection<MusicFileDTO> itemSource;
        private ObservableCollection<MusicFileGroup> groupSource;
        private MusicGrouperArgs grouperArgs;
        private List<MusicItemMenuItem<MusicFileGroupDynamic>> extraGroupMenu;
        private List<MusicItemMenuItem<MusicFileDynamic>> extraItemMenu;

        public MusicGroupArguments(string title = null, IDataServer<MusicFileGroup, KeyValuePair<MusicFileGroup, IEnumerable<MusicFileDTO>>> dataServer = null, ObservableCollection<MusicFileDTO> itemSource = null, ObservableCollection<MusicFileGroup> groupSource = null, MusicGrouperArgs grouperArgs = null, IEnumerable<MusicItemMenuItem<MusicFileGroupDynamic>> extraGroupMenu = null, IEnumerable<MusicItemMenuItem<MusicFileDynamic>> extraItemMenu = null) : base(title)
        {
            if (dataServer != null)
                DataServer = dataServer;
            if (itemSource != null)
                ItemSource = itemSource;
            if (groupSource != null)
                GroupSource = groupSource;
            if (grouperArgs != null)
                GrouperArgs = grouperArgs;
            if (extraGroupMenu != null)
                ExtraGroupMenu = extraGroupMenu.ToList();
            if (extraItemMenu != null)
                ExtraItemMenu = extraItemMenu.ToList();
        }

        public MusicGroupArgsType ArgsType { get; private set; }

        private IDataServer<MusicFileGroup, KeyValuePair<MusicFileGroup, IEnumerable<MusicFileDTO>>> dataServer;
        public IDataServer<MusicFileGroup, KeyValuePair<MusicFileGroup, IEnumerable<MusicFileDTO>>> DataServer
        {
            get => dataServer;
            private set
            {
                dataServer = value;
                SetArgsType(MusicGroupArgsType.DataServer);
            }
        }

        public ObservableCollection<MusicFileDTO> ItemSource
        {
            get => itemSource;
            private set
            {
                itemSource = value;
                SetArgsType(MusicGroupArgsType.ItemSource);
            }
        }

        public ObservableCollection<MusicFileGroup> GroupSource
        {
            get => groupSource;
            private set
            {
                groupSource = value;
                SetArgsType(MusicGroupArgsType.GroupSource);
            }
        }

        public MusicGrouperArgs GrouperArgs
        {
            get => grouperArgs;
            private set
            {
                grouperArgs = value;
                SetArgsType(MusicGroupArgsType.Grouper);
            }
        }

        public List<MusicItemMenuItem<MusicFileGroupDynamic>> ExtraGroupMenu
        {
            get => extraGroupMenu;
            private set
            {
                extraGroupMenu = value;
                SetArgsType(MusicGroupArgsType.GroupMenu);
            }
        }

        public List<MusicItemMenuItem<MusicFileDynamic>> ExtraItemMenu
        {
            get => extraItemMenu;
            private set
            {
                extraItemMenu = value;
                SetArgsType(MusicGroupArgsType.ItemMenu);
            }
        }

        private void SetArgsType(MusicGroupArgsType value)
        {
            ArgsType = ArgsType | value;
        }
    }
}