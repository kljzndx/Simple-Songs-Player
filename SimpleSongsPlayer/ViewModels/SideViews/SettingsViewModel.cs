using Windows.Storage;
using GalaSoft.MvvmLight;

namespace SimpleSongsPlayer.ViewModels.SideViews
{
    public class SettingsViewModel : ViewModelBase
    {
        private StorageLibrary musicLibrary;

        public StorageLibrary MusicLibrary
        {
            get => musicLibrary;
            set => Set(ref musicLibrary, value);
        }
    }
}