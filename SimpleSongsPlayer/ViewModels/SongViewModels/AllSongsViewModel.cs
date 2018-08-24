using System;
using System.Collections.Generic;
using Windows.Globalization.Collation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Models.Factories;

namespace SimpleSongsPlayer.ViewModels.SongViewModels
{
    public class AllSongsViewModel : SongViewModelBase
    {
        public AllSongsViewModel() : base(new AllSongsGroupsFactory())
        {
        }
    }
}