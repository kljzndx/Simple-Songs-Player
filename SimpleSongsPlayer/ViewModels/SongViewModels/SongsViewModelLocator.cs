using GalaSoft.MvvmLight.Ioc;

namespace SimpleSongsPlayer.ViewModels.SongViewModels
{
    public class SongsViewModelLocator
    {
        public SongsViewModelLocator()
        {
            SimpleIoc.Default.Register<SongsViewModel>();
            SimpleIoc.Default.Register<SongArtistsViewModel>();
            SimpleIoc.Default.Register<SongAlbumsViewModel>();
            SimpleIoc.Default.Register<SongsFoldersViewModel>();
            SimpleIoc.Default.Register<PlayingListViewModel>();
        }

        public SongsViewModel Songs => SimpleIoc.Default.GetInstance<SongsViewModel>();
        public SongArtistsViewModel SongArtists => SimpleIoc.Default.GetInstance<SongArtistsViewModel>();
        public SongAlbumsViewModel SongAlbums => SimpleIoc.Default.GetInstance<SongAlbumsViewModel>();
        public SongsFoldersViewModel SongFolders => SimpleIoc.Default.GetInstance<SongsFoldersViewModel>();
        public PlayingListViewModel PlayingList => SimpleIoc.Default.GetInstance<PlayingListViewModel>();
    }
}