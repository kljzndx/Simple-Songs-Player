using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.Factories;

namespace SimpleSongsPlayer.ViewModels.SongViewModels
{
    public abstract class SongViewModelBase : ViewModelBase
    {
        private readonly SongsGroupsFactoryBase songsGroupsFactory;
        private CollectionViewSource songGroups;

        protected SongViewModelBase(SongsGroupsFactoryBase factory)
        {
            songsGroupsFactory = factory;
        }

        public CollectionViewSource SongGroups
        {
            get => songGroups;
            set => Set(ref songGroups, value);
        }

        public void RefreshData(List<Song> allSongs)
        {
            var groups = songsGroupsFactory.ClassifySongsGroups(allSongs);

            SongGroups = new CollectionViewSource
            {
                IsSourceGrouped = true,
                ItemsPath = new PropertyPath("Items"),
                Source = groups
            };
        }
    }
}