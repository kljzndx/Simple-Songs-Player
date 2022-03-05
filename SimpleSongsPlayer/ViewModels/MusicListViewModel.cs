using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

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

        private List<MusicGroup> _source;

        public MusicListViewModel()
        {

        }

        public List<MusicGroup> Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        public MusicFileManageService GetManageService()
        {
            return Ioc.Default.GetRequiredService<MusicFileManageService>();
        }

        public void Load(Func<MusicFileManageService, List<MusicGroup>> sourceGetter)
        {
            this._sourceGetter = sourceGetter;
            Source = _sourceGetter.Invoke(GetManageService());
        }

        public void Refresh()
        {
            if (_sourceGetter != null)
                Source = _sourceGetter.Invoke(GetManageService());
        }
    }
}
