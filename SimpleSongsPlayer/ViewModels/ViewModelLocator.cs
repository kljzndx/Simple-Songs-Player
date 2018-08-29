using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using SimpleSongsPlayer.ViewModels.SongViewModels;

namespace SimpleSongsPlayer.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<LoadingViewModel>();
            SimpleIoc.Default.Register<SongsViewModel>();
            SimpleIoc.Default.Register<SongArtistsViewModel>();
            SimpleIoc.Default.Register<SongAlbumsViewModel>();
        }

        public MainViewModel Main => SimpleIoc.Default.GetInstance<MainViewModel>();
        public LoadingViewModel Loading => SimpleIoc.Default.GetInstance<LoadingViewModel>();
        public SongsViewModel Songs => SimpleIoc.Default.GetInstance<SongsViewModel>();
        public SongArtistsViewModel SongArtists => SimpleIoc.Default.GetInstance<SongArtistsViewModel>();
        public SongAlbumsViewModel SongAlbums => SimpleIoc.Default.GetInstance<SongAlbumsViewModel>();
    }
}