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
        }

        public SongsViewModel Songs => SimpleIoc.Default.GetInstance<SongsViewModel>();
        public SongArtistsViewModel SongArtists => SimpleIoc.Default.GetInstance<SongArtistsViewModel>();
        public SongAlbumsViewModel SongAlbums => SimpleIoc.Default.GetInstance<SongAlbumsViewModel>();
    }
}