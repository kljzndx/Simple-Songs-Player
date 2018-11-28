using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.ViewModels.Factories.MusicFilters;

namespace SimpleSongsPlayer.ViewModels
{
    public class SongListViewModel : ViewModelBase
    {
        private ObservableCollection<MusicFileDTO> original;
        private MusicFilterArgs _filterArgs;
        
        public ObservableCollection<MusicFileDynamic> DataSource { get; } = new ObservableCollection<MusicFileDynamic>();

        public void SetUpDataSource(ObservableCollection<MusicFileDTO> dtos)
        {
            original = dtos;
            
            foreach (var fileDto in original)
                DataSource.Add(new MusicFileDynamic(fileDto));

            original.CollectionChanged -= Original_Normal_CollectionChanged;
            original.CollectionChanged -= Original_Filter_CollectionChanged;

            original.CollectionChanged += Original_Normal_CollectionChanged;
        }

        public void SetUpDataSource(ObservableCollection<MusicFileDTO> dtos, MusicFilterArgs filterArgs)
        {
            original = dtos;
            _filterArgs = filterArgs;
            
            foreach (var fileDynamic in _filterArgs.Filter.Filter(original, _filterArgs.Args))
                DataSource.Add(fileDynamic);

            original.CollectionChanged -= Original_Normal_CollectionChanged;
            original.CollectionChanged -= Original_Filter_CollectionChanged;

            original.CollectionChanged += Original_Filter_CollectionChanged;
        }

        private void Original_Normal_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (MusicFileDTO item in e.NewItems)
                        DataSource.Add(new MusicFileDynamic(item));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in DataSource.Where(d => e.OldItems.Contains(d.Original)))
                        DataSource.Remove(item);
                    break;
            }
        }

        private void Original_Filter_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in _filterArgs.Filter.Filter(e.NewItems.Cast<MusicFileDTO>(), _filterArgs.Args))
                        DataSource.Add(item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in DataSource.Where(d => e.OldItems.Contains(d.Original)))
                        DataSource.Remove(item);
                    break;
            }
        }
    }
}