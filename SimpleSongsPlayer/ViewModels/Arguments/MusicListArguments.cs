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
    public enum MusicListArgsType
    {
        Source = 1,
        Filter = 2,
        Menu = 4
    }

    public class MusicListArguments
    {
        public MusicListArguments(ObservableCollection<MusicFileDTO> source)
        {
            Source = source;
            ArgsType = MusicListArgsType.Source;
        }

        public MusicListArguments(ObservableCollection<MusicFileDTO> source, IEnumerable<MusicItemMenuItem<MusicFileDynamic>> extraMenu)
        {
            Source = source;
            ExtraMenu = extraMenu.ToList();
            ArgsType = MusicListArgsType.Source | MusicListArgsType.Menu;
        }

        public MusicListArguments(ObservableCollection<MusicFileDTO> source, MusicFilterArgs filter)
        {
            Source = source;
            Filter = filter;
            ArgsType = MusicListArgsType.Source | MusicListArgsType.Filter;
        }

        public MusicListArguments(ObservableCollection<MusicFileDTO> source, MusicFilterArgs filter, IEnumerable<MusicItemMenuItem<MusicFileDynamic>> extraMenu)
        {
            Source = source;
            Filter = filter;
            ExtraMenu = extraMenu.ToList();
            ArgsType = MusicListArgsType.Source | MusicListArgsType.Filter | MusicListArgsType.Menu;
        }

        public MusicListArgsType ArgsType { get; }
        public ObservableCollection<MusicFileDTO> Source { get; }
        public MusicFilterArgs Filter { get; }
        public List<MusicItemMenuItem<MusicFileDynamic>> ExtraMenu { get; }
    }
}