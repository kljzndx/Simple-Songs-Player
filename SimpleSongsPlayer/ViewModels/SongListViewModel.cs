using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels
{
    public class SongListViewModel : ViewModelBase
    {
        private ObservableCollection<MusicFileDTO> original;
        
        public ObservableCollection<MusicFileDynamic> DataSource { get; private set; } = new ObservableCollection<MusicFileDynamic>();

        public void SetUpDataSource(ObservableCollection<MusicFileDTO> dtos)
        {
            original = dtos;

            foreach (var fileDto in original)
                DataSource.Add(new MusicFileDynamic(fileDto));

            original.CollectionChanged += Original_CollectionChanged;
        }

        private void Original_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
    }
}