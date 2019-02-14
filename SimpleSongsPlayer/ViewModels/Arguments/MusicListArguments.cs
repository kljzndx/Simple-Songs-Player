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
    public enum MusicListArgsType
    {
        Source = 1,
        Filter = 2,
        Menu = 4,
        DataServer = 8
    }

    public class MusicListArguments : PageArgumentsBase
    {
        public MusicListArguments(string title = null, IFileDataServer<MusicFileDTO> dataServer = null, ObservableCollection<MusicFileDTO> source = null, MusicFilterArgs filter = null, IEnumerable<MusicItemMenuItem<MusicFileDynamic>> extraMenu = null, bool isEnableViewOption = true) : base(title)
        {
            DataServer = dataServer;
            Source = source;
            Filter = filter;
            ExtraMenu = extraMenu?.ToList();
            IsEnableViewOption = isEnableViewOption;
        }

        public MusicListArgsType ArgsType
        {
            get
            {
                MusicListArgsType args = default(MusicListArgsType);

                if (Source != null)
                    args = MusicListArgsType.Source;

                if (DataServer != null)
                    args |= MusicListArgsType.DataServer;

                if (Filter != null)
                    args |= MusicListArgsType.Filter;

                if (ExtraMenu != null)
                    args |= MusicListArgsType.Menu;
                
                return args;
            }
        }

        public IFileDataServer<MusicFileDTO> DataServer { get; }
        public ObservableCollection<MusicFileDTO> Source { get; }
        public MusicFilterArgs Filter { get; }
        public List<MusicItemMenuItem<MusicFileDynamic>> ExtraMenu { get; }
        public bool IsEnableViewOption { get; }
    }
}