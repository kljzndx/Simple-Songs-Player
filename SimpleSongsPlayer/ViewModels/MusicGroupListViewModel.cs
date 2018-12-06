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

namespace SimpleSongsPlayer.ViewModels
{
    public class MusicGroupListViewModel : ViewModelBase
    {
        private IMusicGrouper _grouper;

        private ObservableCollection<MusicFileGroup> dataSource;

        public ObservableCollection<MusicFileDTO> Original { get; private set; }
        public IMusicFilter ItemFilter { get; private set; }

        public ObservableCollection<MusicFileGroup> DataSource
        {
            get => dataSource;
            private set => Set(ref dataSource, value);
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
                            dataSource.Add(fileGroup);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (MusicFileGroup group in e.OldItems)
                        if (dataSource.FirstOrDefault(g => g.Name == group.Name) is MusicFileGroup current)
                            foreach (var item in group.Items)
                                current.Items.Remove(item);
                    break;
            }
        }
    }
}