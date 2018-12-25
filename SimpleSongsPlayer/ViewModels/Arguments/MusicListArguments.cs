﻿using System;
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
        public MusicListArguments(ObservableCollection<MusicFileDTO> source, string pageTitle = null) : base(pageTitle)
        {
            Source = source;
            ArgsType = MusicListArgsType.Source;
        }

        public MusicListArguments(ObservableCollection<MusicFileDTO> source, IFileDataServer<MusicFileDTO> dataServer, string pageTitle = null) : base(pageTitle)
        {
            Source = source;
            DataServer = dataServer;
            ArgsType = MusicListArgsType.Source | MusicListArgsType.DataServer;
        }

        public MusicListArguments(ObservableCollection<MusicFileDTO> source, IEnumerable<MusicItemMenuItem<MusicFileDynamic>> extraMenu, string pageTitle = null) : base(pageTitle)
        {
            Source = source;
            ExtraMenu = extraMenu.ToList();
            ArgsType = MusicListArgsType.Source | MusicListArgsType.Menu;
        }

        public MusicListArguments(ObservableCollection<MusicFileDTO> source, IFileDataServer<MusicFileDTO> dataServer, IEnumerable<MusicItemMenuItem<MusicFileDynamic>> extraMenu, string pageTitle = null) : base(pageTitle)
        {
            Source = source;
            DataServer = dataServer;
            ExtraMenu = extraMenu.ToList();
            ArgsType = MusicListArgsType.Source | MusicListArgsType.DataServer | MusicListArgsType.Menu;
        }

        public MusicListArguments(ObservableCollection<MusicFileDTO> source, MusicFilterArgs filter, string pageTitle = null) : base(pageTitle)
        {
            Source = source;
            Filter = filter;
            ArgsType = MusicListArgsType.Source | MusicListArgsType.Filter;
        }

        public MusicListArguments(ObservableCollection<MusicFileDTO> source, IFileDataServer<MusicFileDTO> dataServer, MusicFilterArgs filter, string pageTitle = null) : base(pageTitle)
        {
            Source = source;
            DataServer = dataServer;
            Filter = filter;
            ArgsType = MusicListArgsType.Source | MusicListArgsType.DataServer | MusicListArgsType.Filter;
        }

        public MusicListArguments(ObservableCollection<MusicFileDTO> source, MusicFilterArgs filter, IEnumerable<MusicItemMenuItem<MusicFileDynamic>> extraMenu, string pageTitle = null) : base(pageTitle)
        {
            Source = source;
            Filter = filter;
            ExtraMenu = extraMenu.ToList();
            ArgsType = MusicListArgsType.Source | MusicListArgsType.Filter | MusicListArgsType.Menu;
        }

        public MusicListArguments(ObservableCollection<MusicFileDTO> source, IFileDataServer<MusicFileDTO> dataServer, MusicFilterArgs filter, IEnumerable<MusicItemMenuItem<MusicFileDynamic>> extraMenu, string pageTitle = null) : base(pageTitle)
        {
            Source = source;
            DataServer = dataServer;
            Filter = filter;
            ExtraMenu = extraMenu.ToList();
            ArgsType = MusicListArgsType.Source | MusicListArgsType.DataServer | MusicListArgsType.Filter | MusicListArgsType.Menu;
        }

        public MusicListArgsType ArgsType { get; }
        public IFileDataServer<MusicFileDTO> DataServer { get; }
        public ObservableCollection<MusicFileDTO> Source { get; }
        public MusicFilterArgs Filter { get; }
        public List<MusicItemMenuItem<MusicFileDynamic>> ExtraMenu { get; }
    }
}