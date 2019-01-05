using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.ViewModels.Factories;
using SimpleSongsPlayer.ViewModels.Factories.MusicFilters;
using SimpleSongsPlayer.ViewModels.Factories.MusicGroupers;
using SimpleSongsPlayer.ViewModels.SettingProperties;

namespace SimpleSongsPlayer.ViewModels
{
    public class MusicGroupListViewModel : ViewModelBase
    {
        private IMusicGrouper _grouper;

        private ObservableCollection<MusicFileGroup> dataSource;

        public MusicGroupListViewModel()
        {
            SorterMembers = new List<MusicSorterUi<MusicFileGroup>>
            {
                new MusicSorterUi<MusicFileGroup>("Title", g => g.Name),
                new MusicSorterUi<MusicFileGroup>("Count", g => g.Items.Count, true)
            };
        }

        public ObservableCollection<MusicFileDTO> Original { get; private set; }
        public IMusicFilter ItemFilter { get; private set; }

        public ObservableCollection<MusicFileGroup> DataSource
        {
            get => dataSource;
            private set => Set(ref dataSource, value);
        }

        public List<MusicSorterUi<MusicFileGroup>> SorterMembers { get; }

        public void Sort(MusicSorterUi<MusicFileGroup> sorter)
        {
            var sortList = dataSource.OrderBy(sorter.KeySelector.Invoke).ToList();
            if (sorter.IsReverse)
                sortList.Reverse();

            foreach (var fileGroup in sortList)
            {
                int oldId = dataSource.IndexOf(fileGroup);
                int newId = sortList.IndexOf(fileGroup);

                dataSource.Move(oldId, newId);
            }
        }

        public void Reverse()
        {
            var sortList = dataSource.Reverse().ToList();

            foreach (var fileGroup in sortList)
            {
                int oldId = dataSource.IndexOf(fileGroup);
                int newId = sortList.IndexOf(fileGroup);

                dataSource.Move(oldId, newId);
            }
        }

        public void AutoSort()
        {
            Sort(SorterMembers[(int)MusicGroupViewSettingProperties.Current.SortMethod]);
            if (MusicGroupViewSettingProperties.Current.IsReverse)
                Reverse();
        }

        public void SetUp(ObservableCollection<MusicFileGroup> source)
        {
            DataSource = source;
        }

        public void SetUp(ObservableCollection<MusicFileDTO> source, MusicGrouperArgs grouperArgs)
        {
            Original = source;
            _grouper = grouperArgs.Grouper;
            ItemFilter = grouperArgs.ItemFilter;

            DataSource = new ObservableCollection<MusicFileGroup>(_grouper.Group(Original));

            Original.CollectionChanged += Original_CollectionChanged;
        }

        private void Original_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var fileGroup in _grouper.Group(e.NewItems.Cast<MusicFileDTO>()))
                        if (dataSource.FirstOrDefault(g => g.Name == fileGroup.Name) is MusicFileGroup group)
                            group.Join(fileGroup);
                        else
                        {
                            dataSource.Add(fileGroup);

                            AutoSort();
                        }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var groups = _grouper.Group(e.OldItems.Cast<MusicFileDTO>())
                        .Where(g => dataSource.Any(d => d.Name == g.Name));

                    foreach (var fileGroup in groups)
                    {
                        var theGroup = dataSource.First(g => g.Name == fileGroup.Name);
                        if (fileGroup.Items.Count >= theGroup.Items.Count)
                            dataSource.Remove(theGroup);
                        else
                            foreach (var fileDto in fileGroup.Items)
                                theGroup.Items.Remove(fileDto);
                    }
                    break;
            }
        }
    }
}