using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.ViewModels.Factories;
using SimpleSongsPlayer.ViewModels.Factories.MusicGroupers;

namespace SimpleSongsPlayer.ViewModels
{
    public class MusicListViewModel : ViewModelBase
    {
        private ObservableCollection<MusicFileDTO> original;
        private MusicFilterArgs _filterArgs;
        private IMusicGrouper _grouper = new SingleGrouper();

        public ObservableCollection<MusicFileGroupDynamic> DataSource { get; } = new ObservableCollection<MusicFileGroupDynamic>();

        public void GroupItems(IEnumerable<MusicFileDTO> fileDtos = null, bool needClear = false, IMusicGrouper grouper = null)
        {
            var source = fileDtos ?? (_filterArgs != null ? _filterArgs.Filter.Filter(original, _filterArgs.Args) : original);
            if (needClear) DataSource.Clear();
            if (grouper != null) _grouper = grouper;
            
            foreach (var item in _grouper.Group(source))
                if (DataSource.FirstOrDefault(f => f.Name == item.Name) is MusicFileGroupDynamic groupDynamic)
                    groupDynamic.Join(item);
                else
                    DataSource.Add(new MusicFileGroupDynamic(item));
        }

        public void SortItems(MusicDynamicSortKeySelector keySelector)
        {
            foreach (var groupDynamic in DataSource)
               groupDynamic.OrderBy(keySelector);
        }

        public void SetUpDataSource(ObservableCollection<MusicFileDTO> dtos)
        {
            original = dtos;

            GroupItems(original);
            
            original.CollectionChanged -= Original_Normal_CollectionChanged;
            original.CollectionChanged -= Original_Filter_CollectionChanged;

            original.CollectionChanged += Original_Normal_CollectionChanged;
        }

        public void SetUpDataSource(ObservableCollection<MusicFileDTO> dtos, MusicFilterArgs filterArgs)
        {
            original = dtos;
            _filterArgs = filterArgs;

            GroupItems(_filterArgs.Filter.Filter(original, filterArgs.Args));

            original.CollectionChanged -= Original_Normal_CollectionChanged;
            original.CollectionChanged -= Original_Filter_CollectionChanged;

            original.CollectionChanged += Original_Filter_CollectionChanged;
        }

        private void RemoveDtos(IEnumerable<MusicFileDTO> datas)
        {
            foreach (var data in datas)
            {
                var groupDynamic = DataSource.First(g => g.Items.Any(f => f.Original.FilePath == data.FilePath));
                groupDynamic.Items.Remove(groupDynamic.Items.First(f => f.Original.FilePath == data.FilePath));
            }
        }

        private void Original_Normal_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    GroupItems(e.NewItems.Cast<MusicFileDTO>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var datas = e.OldItems.Cast<MusicFileDTO>();
                    RemoveDtos(datas);
                    break;
            }
        }

        private void Original_Filter_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    GroupItems(_filterArgs.Filter.Filter(e.NewItems.Cast<MusicFileDTO>(), _filterArgs.Args));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var datas = e.OldItems.Cast<MusicFileDTO>();
                    RemoveDtos(datas);
                    break;
            }
        }
    }
}