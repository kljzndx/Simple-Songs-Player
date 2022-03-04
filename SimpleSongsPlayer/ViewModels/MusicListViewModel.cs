using CommunityToolkit.Mvvm.ComponentModel;

using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.ViewModels
{
    public class MusicListViewModel : ObservableRecipient
    {
        private Func<MusicFileManageService, List<MusicGroup>> _sourceGetter;
        private MusicFileManageService _manageService;

        private List<MusicGroup> _source;

        public MusicListViewModel(MusicFileManageService manageService)
        {
            _manageService = manageService;
        }

        public List<MusicGroup> Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        public void Load(Func<MusicFileManageService, List<MusicGroup>> sourceGetter)
        {
            this._sourceGetter = sourceGetter;
            Source = _sourceGetter.Invoke(_manageService);
        }

        public void Refresh()
        {
            Source = _sourceGetter.Invoke(_manageService);
        }
    }
}
