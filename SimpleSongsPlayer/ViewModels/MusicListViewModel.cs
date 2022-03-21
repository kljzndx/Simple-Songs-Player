using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.ViewModels
{
    public class MusicListViewModel : ObservableRecipient
    {
        private MusicFileManageService _manageService;

        private IEnumerable<MusicUi> _musicListSource;
        private List<DataSourceItem> _dataSourceList;
        private IEnumerable<MusicGroup> _source;

        public MusicListViewModel(ConfigurationService configService, MusicFileManageService manageService, PlaybackListManageService playbackListService)
        {
            ConfigService = configService;
            _manageService = manageService;
            PlaybackListService = playbackListService;

            GenerateDataSource();
            AutoImport();

            Messenger.Register<MusicListViewModel, string, string>(this, nameof(MusicFileScanningService), (vm, message) =>
            {
                if (message == "Finished")
                {
                    GenerateDataSource();
                    vm.AutoImport();
                }
            });

            ConfigService.PropertyChanged += ConfigService_PropertyChanged;
        }

        public List<DataSourceItem> DataSourceList
        {
            get => _dataSourceList;
            set => SetProperty(ref _dataSourceList, value);
        }

        public IEnumerable<MusicGroup> Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        public ConfigurationService ConfigService { get; }
        public PlaybackListManageService PlaybackListService { get; }

        public MusicGroup GetGroupFromItem(MusicUi item)
        {
            return Source.First(mg => mg.Items.Contains(item));
        }

        public void GenerateDataSource()
        {
            var list = new List<DataSourceItem>();

            list.Add(new DataSourceItem("All", ms => ms.GetAllMusic(), false));
            list.Add(new DataSourceItem("Playback list", ms => ms.GetPlaybackList(), false));

            var libs = _manageService.GetLibraryListInDb();
            list.AddRange(libs.Select(lib => new DataSourceItem(lib, ms => ms.QueryMusicByLibraryPath(lib), true)));

            DataSourceList = list;
        }

        public void AutoImport()
        {
            int id;

            if (ConfigService.DataSourceId < 0)
                id = 0;
            else if (ConfigService.DataSourceId >= DataSourceList.Count)
                id = DataSourceList.Count - 1;
            else
                id = ConfigService.DataSourceId;

            _musicListSource = DataSourceList[id].QueryAction(_manageService);

            AutoGroup();
            SortGroup();
            SortList();
        }

        public void AutoGroup()
        {
            switch (ConfigService.MusicListGroupMethod)
            {
                case MusicListGroupMethodEnum.None:
                    Source = new List<MusicGroup>(new[] { new MusicGroup("All", _musicListSource) });
                    break;
                case MusicListGroupMethodEnum.FirstCharacterOfTitle:
                    Source = _manageService.GroupMusicByFirstLetter(_musicListSource);
                    break;
                case MusicListGroupMethodEnum.Artist:
                    Source = _manageService.GroupMusic(_musicListSource, mu => mu.Artist);
                    break;
                case MusicListGroupMethodEnum.Album:
                    Source = _manageService.GroupMusic(_musicListSource, mu => mu.Album);
                    break;
                case MusicListGroupMethodEnum.Folder:
                    Source = _manageService.GroupMusic(_musicListSource, mu => Path.GetDirectoryName(mu.FilePath));
                    break;
            }
        }

        public void SortGroup()
        {
            if (Source == null || !Source.Any()) return;

            switch (ConfigService.MusicGroupListSort)
            {
                case MusicGroupListSortEnum.Name:
                    Source = _source.OrderBy(mg => mg.Name).ToList();
                    break;
                case MusicGroupListSortEnum.Count:
                    Source = _source.OrderBy(mg => mg.Count).ToList();
                    break;
            }

            if (ConfigService.IsReverseMusicGroupList)
                Source = _source.Reverse().ToList();
        }

        public void SortList()
        {
            if (Source == null || !Source.Any()) return;

            switch (ConfigService.MusicListSort)
            {
                case MusicListSortEnum.Title:
                    Source = _manageService.OrderMusicDataBy(_source, m => m.Title);
                    break;
                case MusicListSortEnum.TrackNumber:
                    Source = _manageService.OrderMusicDataBy(_source, m => m.TrackNumber);
                    break;
                case MusicListSortEnum.ModifyDate:
                    Source = _manageService.OrderMusicDataBy(_source, m => m.ModifyDate);
                    break;
            }

            if (ConfigService.IsReverseMusicList)
                Source = _manageService.ReverseMusicData(_source);
        }

        private void ConfigService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ConfigService.DataSourceId):
                    AutoImport();
                    break;
                case nameof(ConfigService.MusicListGroupMethod):
                    AutoGroup();
                    break;
                case nameof(ConfigService.IsReverseMusicGroupList):
                case nameof(ConfigService.MusicGroupListSort):
                    SortGroup();
                    break;
                case nameof(ConfigService.IsReverseMusicList):
                case nameof(ConfigService.MusicListSort):
                    if (ConfigService.MusicListSort == MusicListSortEnum.None)
                        AutoImport();
                    else
                        SortList();
                    break;
            }
        }
    }
}
