using System;
using System.Collections.Generic;
using Windows.Globalization.Collation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Models.Factories;

namespace SimpleSongsPlayer.ViewModels.SongViewModels
{
    public class SongsViewModel : SongViewModelBase
    {
        public SongsViewModel() : base(new AllSongsGroupsFactory())
        {
        }
    }
}