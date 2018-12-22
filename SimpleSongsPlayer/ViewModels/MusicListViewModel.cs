﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.ViewModels.DataServers;
using SimpleSongsPlayer.ViewModels.Factories;
using SimpleSongsPlayer.ViewModels.Factories.MusicGroupers;
using SimpleSongsPlayer.ViewModels.SettingProperties;

namespace SimpleSongsPlayer.ViewModels
{
    public class MusicListViewModel : ViewModelBase
    {
        private ObservableCollection<MusicFileDTO> original;
        private MusicFilterArgs _filterArgs;
        private IMusicGrouper _grouper = new SingleGrouper();
        private readonly MusicViewSettingProperties settings = MusicViewSettingProperties.Current;

        public MusicListViewModel()
        {
            GrouperMembers = new List<MusicGrouperUi>();
            SorterMembers = new List<MusicSorterUi>();

            GrouperMembers.Add(new MusicGrouperUi("GrouperMember_None", new SingleGrouper()));
            GrouperMembers.Add(new MusicGrouperUi("GrouperMember_FirstLetter", new CharacterGrouper()));

            SorterMembers.Add(new MusicSorterUi("SorterMember_Title", s => s.Original.Title));
            SorterMembers.Add(new MusicSorterUi("SorterMember_Artist", s => s.Original.Artist));
            SorterMembers.Add(new MusicSorterUi("SorterMember_Album", s => s.Original.Album));
            SorterMembers.Add(new MusicSorterUi("SorterMember_ChangeDate", s => s.Original.ChangeDate, true));

            settings.PropertyChanged += Settings_PropertyChanged;
            MusicLibraryDataServer.Current.MusicFilesAdded += DataServer_MusicFilesAdded;
        }

        public List<MusicGrouperUi> GrouperMembers { get; }
        public List<MusicSorterUi> SorterMembers { get; }
        
        public ObservableCollection<MusicFileGroupDynamic> DataSource { get; } = new ObservableCollection<MusicFileGroupDynamic>();

        private void AddItems(IEnumerable<MusicFileDTO> fileDtos = null, bool needClear = false, IMusicGrouper grouper = null)
        {
            var source = fileDtos ?? (_filterArgs != null ? _filterArgs.Filter.Filter(original, _filterArgs.Args) : original);
            if (needClear) DataSource.Clear();
            _grouper = grouper ?? GrouperMembers[(int) settings.GroupMethod].Grouper;
            
            foreach (var item in _grouper.Group(source))
                if (DataSource.FirstOrDefault(f => f.Name == item.Name) is MusicFileGroupDynamic groupDynamic)
                    groupDynamic.Join(item);
                else
                    DataSource.Add(new MusicFileGroupDynamic(item));
        }

        private void RemoveDtos(IEnumerable<MusicFileDTO> datas)
        {
            foreach (var data in datas)
            {
                var groupDynamic = DataSource.First(g => g.Items.Any(f => f.Original.FilePath == data.FilePath));
                groupDynamic.Items.Remove(groupDynamic.Items.First(f => f.Original.FilePath == data.FilePath));
            }
        }

        private void SortItems(MusicDynamicSortKeySelector keySelector)
        {
            foreach (var groupDynamic in DataSource)
                groupDynamic.OrderBy(keySelector);
        }

        private void ReverseItems()
        {
            foreach (var groupDynamic in DataSource)
                groupDynamic.ReverseItems();
        }

        public IEnumerable<MusicFileDynamic> GetAllMusic()
        {
            return DataSource.Select(g => g.Items).Aggregate((o, n) =>
            {
                var list = new ObservableCollection<MusicFileDynamic>(o);
                foreach (var fileDynamic in n)
                    list.Add(fileDynamic);
                return list;
            });
        }

        public void SetUpDataSource(ObservableCollection<MusicFileDTO> dtos)
        {
            original = dtos;

            AddItems(original);

            original.CollectionChanged -= Original_Normal_CollectionChanged;
            original.CollectionChanged -= Original_Filter_CollectionChanged;

            original.CollectionChanged += Original_Normal_CollectionChanged;
        }

        public void SetUpDataSource(ObservableCollection<MusicFileDTO> dtos, MusicFilterArgs filterArgs)
        {
            original = dtos;
            _filterArgs = filterArgs;

            AddItems(_filterArgs.Filter.Filter(original, filterArgs.Args));
            
            original.CollectionChanged -= Original_Normal_CollectionChanged;
            original.CollectionChanged -= Original_Filter_CollectionChanged;

            original.CollectionChanged += Original_Filter_CollectionChanged;
        }

        private void Original_Normal_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(e.NewItems.Cast<MusicFileDTO>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveDtos(e.OldItems.Cast<MusicFileDTO>());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (!original.Any())
                        DataSource.Clear();
                    break;
            }
        }

        private void Original_Filter_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(_filterArgs.Filter.Filter(e.NewItems.Cast<MusicFileDTO>(), _filterArgs.Args));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var datas = e.OldItems.Cast<MusicFileDTO>();
                    RemoveDtos(datas);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (!original.Any())
                        DataSource.Clear();
                    break;
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(settings.GroupMethod):
                    AddItems(needClear: true, grouper: GrouperMembers[(int)settings.GroupMethod].Grouper);
                    SortItems(SorterMembers[(int)settings.SortMethod].KeySelector);
                    if (settings.IsReverse)
                        ReverseItems();
                    break;
                case nameof(settings.SortMethod):
                    var sorter = SorterMembers[(int)settings.SortMethod];
                    SortItems(sorter.KeySelector);
                    settings.IsReverse = sorter.IsReverse;
                    break;
                case nameof(settings.IsReverse):
                    if (settings.IsReverse)
                        ReverseItems();
                    else
                        SortItems(SorterMembers[(int)settings.SortMethod].KeySelector);

                    break;
            }
        }

        private void DataServer_MusicFilesAdded(object sender, System.EventArgs e)
        {
            var sorter = SorterMembers[(int)settings.SortMethod];
            SortItems(sorter.KeySelector);
            if (settings.IsReverse)
                ReverseItems();
        }
    }
}