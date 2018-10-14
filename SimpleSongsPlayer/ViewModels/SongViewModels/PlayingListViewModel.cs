using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.Factories;
using SimpleSongsPlayer.Operator;

namespace SimpleSongsPlayer.ViewModels.SongViewModels
{
    public class PlayingListViewModel : SongViewModelBase
    {
        private PlayingListManager listManager;

        public PlayingListViewModel() : base(new PlayingListFactory())
        {
        }

        public override async Task RefreshData(List<Song> allSongs)
        {
            if (listManager is null)
                listManager = await PlayingListManager.GetManager();

            if (SongGroups != null)
            {
                foreach (var songGroup in SongGroups)
                    songGroup.Items.CollectionChanged -= GroupItems_CollectionChanged;

                SongGroups.CollectionChanged -= SongGroups_CollectionChanged;
            }

            await base.RefreshData(allSongs);

            SongGroups.CollectionChanged += SongGroups_CollectionChanged;

            foreach (var songsGroup in SongGroups)
                songsGroup.Items.CollectionChanged += GroupItems_CollectionChanged;
        }

        private async void SongGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (listManager is null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (SongsGroup group in e.NewItems)
                        group.Items.CollectionChanged += GroupItems_CollectionChanged;

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (SongsGroup group in e.OldItems)
                    {
                        if (listManager.Blocks.FirstOrDefault(b => b.Name.Equals(@group.Name)) is PlayingListBlock block)
                            await listManager.DeleteBlockAsync(block);

                        group.Items.CollectionChanged -= GroupItems_CollectionChanged;
                    }
                    break;
            }
        }

        private async void GroupItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add && e.Action != NotifyCollectionChangedAction.Remove)
                return;

            bool isAdd = e.Action == NotifyCollectionChangedAction.Add;

            var theGroup = sender as SongsGroup;
            if (theGroup is null)
                return;

            string groupName = theGroup.Name;
            var theBlock = listManager.GetBlock(groupName);
            if (theBlock is null)
                throw new Exception("未找到该歌曲所属的列表");

            foreach (Song item in e.NewItems)
                if (isAdd)
                    await theBlock.AddPath(item.Path);
                else
                    await theBlock.RemovePath(item.Path);
        }
    }
}