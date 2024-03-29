﻿using CommunityToolkit.Mvvm.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.ViewModels
{
    public class ViewModelLocator
    {
        public MainViewModel Main => Ioc.Default.GetRequiredService<MainViewModel>();
        public MusicListViewModel MusicList => Ioc.Default.GetRequiredService<MusicListViewModel>();
        public MusicInfoViewModel MusicInfo => Ioc.Default.GetRequiredService<MusicInfoViewModel>();
    }
}
