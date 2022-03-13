using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.ViewModels
{
    public class MusicListViewModel : ObservableRecipient
    {
        private Func<MusicFileManageService, List<MusicGroup>> _sourceGetter;
        private MusicFileManageService _manageService;

        private IEnumerable<MusicGroup> _source;

        public MusicListViewModel(ConfigurationService configService, MusicFileManageService manageService, PlaybackListManageService playbackListService)
        {
            ConfigService = configService;
            _manageService = manageService;
            PlaybackListService = playbackListService;

            Messenger.Register<MusicListViewModel, string, string>(this, nameof(MusicFileScanningService),
                (vm, message) => { if (message == "Finished") vm.Refresh(); });

            ConfigService.PropertyChanged += ConfigService_PropertyChanged;
        }

        public IEnumerable<MusicGroup> Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        public ConfigurationService ConfigService { get; }
        public PlaybackListManageService PlaybackListService { get; }

        public void Load(Func<MusicFileManageService, List<MusicGroup>> sourceGetter)
        {
            this._sourceGetter = sourceGetter;
            Refresh();
        }

        public void Refresh()
        {
            if (_sourceGetter != null)
                Source = _sourceGetter.Invoke(_manageService);

            SortGroup();
            SortList();
        }

        public void SortGroup()
        {
            if (Source == null || !Source.Any()) return;

            switch (ConfigService.MusicGroupListSort)
            {
                case MusicGroupSortEnum.Name:
                    Source = _source.OrderBy(mg => mg.Name).ToList();
                    break;
                case MusicGroupSortEnum.Count:
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
                case nameof(ConfigService.IsReverseMusicGroupList):
                case nameof(ConfigService.MusicGroupListSort):
                    SortGroup();
                    break;
                case nameof(ConfigService.IsReverseMusicList):
                case nameof(ConfigService.MusicListSort):
                    SortList();
                    break;
            }
        }
    }
}
