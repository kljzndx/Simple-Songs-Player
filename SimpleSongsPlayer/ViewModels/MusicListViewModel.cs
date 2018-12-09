using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
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

            SorterMembers.Add(new MusicSorterUi("SorterMember_Title", (s, c) => s.Original.Title));
            SorterMembers.Add(new MusicSorterUi("SorterMember_Artist", (s, c) => s.Original.Artist));
            SorterMembers.Add(new MusicSorterUi("SorterMember_Album", (s, c) => s.Original.Album));
            SorterMembers.Add(new MusicSorterUi("SorterMember_ChangeDate", (s, c) => s.Original.ChangeDate, true));

            settings.PropertyChanged += Settings_PropertyChanged;
            DataServer.Current.MusicFilesAdded += DataServer_MusicFilesAdded;
        }

        private void DataServer_MusicFilesAdded(object sender, System.EventArgs e)
        {
            var sorter = SorterMembers[(int)settings.SortMethod];
            SortItems(sorter.KeySelector);
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

        public void SortItems(MusicDynamicSortKeySelector keySelector, bool? isReverse = null)
        {
            bool r = isReverse ?? settings.IsReverse;
            foreach (var groupDynamic in DataSource)
            {
                groupDynamic.OrderBy(keySelector);
                if (r)
                    groupDynamic.ReverseItems();
            }
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
                    AddItems(_filterArgs.Filter.Filter(e.NewItems.Cast<MusicFileDTO>(), _filterArgs.Args));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var datas = e.OldItems.Cast<MusicFileDTO>();
                    RemoveDtos(datas);
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
                    break;
                case nameof(settings.SortMethod):
                    var sorter = SorterMembers[(int)settings.SortMethod];
                    SortItems(sorter.KeySelector);
                    settings.IsReverse = sorter.IsReverse;
                    break;
                case nameof(settings.IsReverse):
                    SortItems(SorterMembers[(int)settings.SortMethod].KeySelector, settings.IsReverse);
                    break;
            }
        }
    }
}