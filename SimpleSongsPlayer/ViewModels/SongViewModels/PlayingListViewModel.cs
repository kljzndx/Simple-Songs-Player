using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.Events;
using SimpleSongsPlayer.Models.Factories;
using SimpleSongsPlayer.Operator;

namespace SimpleSongsPlayer.ViewModels.SongViewModels
{
    public class PlayingListViewModel : SongViewModelBase
    {
        private PlayingListManager listManager;
        private List<Song> allSongsList;

        public PlayingListViewModel() : base(new PlayingListFactory())
        {
        }

        public async Task RefreshData()
        {
            if (allSongsList is null)
                return;

            await RefreshData(allSongsList);
        }

        public override async Task RefreshData(List<Song> allSongs)
        {
            allSongsList = allSongs;

            if (listManager is null)
            {
                LoggerMembers.VmLogger.Info("正在初始化播放列表管理器");
                listManager = await PlayingListManager.GetManager();
                LoggerMembers.VmLogger.Info("完成初始化");
            }

            if (SongGroups != null)
            {
                LoggerMembers.VmLogger.Info("正在取消歌曲组监听");
                foreach (var songGroup in SongGroups)
                {
                    songGroup.Renamed -= Group_Renamed;
                    songGroup.ItemRemoveRequested -= GroupItem_RemoveRequested;
                    songGroup.Items.CollectionChanged -= GroupItems_CollectionChanged;
                }

                SongGroups.CollectionChanged -= SongGroups_CollectionChanged;
                LoggerMembers.VmLogger.Info("完成取消歌曲组监听");
            }

            await base.RefreshData(allSongs);

            LoggerMembers.VmLogger.Info("正在取消歌曲组监听");
            SongGroups.CollectionChanged += SongGroups_CollectionChanged;

            foreach (var songsGroup in SongGroups)
            {
                songsGroup.Renamed += Group_Renamed;
                songsGroup.ItemRemoveRequested += GroupItem_RemoveRequested;
                songsGroup.Items.CollectionChanged += GroupItems_CollectionChanged;
            }
            LoggerMembers.VmLogger.Info("完成取消歌曲组监听");
        }

        private async void SongGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (listManager is null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (SongsGroup group in e.NewItems)
                    {
                        LoggerMembers.VmLogger.Info("监听新添加的歌曲组");
                        group.Renamed -= Group_Renamed;
                        group.ItemRemoveRequested -= GroupItem_RemoveRequested;
                        group.Items.CollectionChanged -= GroupItems_CollectionChanged;

                        group.Renamed += Group_Renamed;
                        group.ItemRemoveRequested += GroupItem_RemoveRequested;
                        group.Items.CollectionChanged += GroupItems_CollectionChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (SongsGroup group in e.OldItems)
                    {
                        LoggerMembers.VmLogger.Info("正在删除播放列表");
                        if (listManager.GetBlocks().FirstOrDefault(b => b.Name.Equals(@group.Name)) is PlayingListBlock block)
                            await listManager.DeleteBlockAsync(block);

                        LoggerMembers.VmLogger.Info("完成播放列表删除， 正在取消监听");
                        group.Renamed -= Group_Renamed;
                        group.ItemRemoveRequested -= GroupItem_RemoveRequested;
                        group.Items.CollectionChanged -= GroupItems_CollectionChanged;
                        LoggerMembers.VmLogger.Info("完成取消监听");
                    }
                    break;
            }
        }

        private async void Group_Renamed(SongsGroup sender, SongGroupRenamedEventArgs args)
        {
            LoggerMembers.VmLogger.Info("正在重命名播放列表");
            var theBlock = listManager.GetBlock(args.OldName);
            await theBlock.RenameAsync(args.NewName);
            LoggerMembers.VmLogger.Info("完成播放列表重命名");
        }

        private void GroupItem_RemoveRequested(SongsGroup sender, SongItem args)
        {
            if (!sender.IsAny)
            {
                LoggerMembers.VmLogger.Info("删除空播放列表");
                SongGroups.Remove(sender);
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

            foreach (SongItem item in e.NewItems)
                if (isAdd)
                {
                    LoggerMembers.VmLogger.Info("正在添加歌曲至播放列表");
                    await theBlock.AddPath(item.Path);
                    LoggerMembers.VmLogger.Info("完成歌曲添加");
                }
                else
                {
                    LoggerMembers.VmLogger.Info("正在从播放列表删除歌曲");
                    await theBlock.RemovePath(item.Path);
                    LoggerMembers.VmLogger.Info("完成歌曲删除");
                }
        }
    }
}