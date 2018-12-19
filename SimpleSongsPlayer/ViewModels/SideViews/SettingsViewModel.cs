using System;
using System.Threading.Tasks;
using Windows.Storage;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.ViewModels.DataServers;

namespace SimpleSongsPlayer.ViewModels.SideViews
{
    public class SettingsViewModel : ViewModelBase
    {
        private StorageLibrary musicLibrary;
        private bool isSubmitting;

        public StorageLibrary MusicLibrary
        {
            get => musicLibrary;
            set => Set(ref musicLibrary, value);
        }

        public bool IsSubmitting
        {
            get => isSubmitting;
            set => Set(ref isSubmitting, value);
        }
    }
}