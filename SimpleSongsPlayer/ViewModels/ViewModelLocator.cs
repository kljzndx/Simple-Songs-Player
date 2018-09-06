using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using SimpleSongsPlayer.ViewModels.Controllers;
using SimpleSongsPlayer.ViewModels.SongViewModels;

namespace SimpleSongsPlayer.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<FrameworkViewModel>();
            SimpleIoc.Default.Register<AllSongListsViewModel>();
            SimpleIoc.Default.Register<LoadingViewModel>();
            SimpleIoc.Default.Register<SongsViewModel>();
            SimpleIoc.Default.Register<SongArtistsViewModel>();
            SimpleIoc.Default.Register<SongAlbumsViewModel>();
            SimpleIoc.Default.Register<PlayerControllerViewModel>();
        }

        public FrameworkViewModel Framework => SimpleIoc.Default.GetInstance<FrameworkViewModel>();
        public AllSongListsViewModel AllSongLists => SimpleIoc.Default.GetInstance<AllSongListsViewModel>();
        public LoadingViewModel Loading => SimpleIoc.Default.GetInstance<LoadingViewModel>();
        public SongsViewModel Songs => SimpleIoc.Default.GetInstance<SongsViewModel>();
        public SongArtistsViewModel SongArtists => SimpleIoc.Default.GetInstance<SongArtistsViewModel>();
        public SongAlbumsViewModel SongAlbums => SimpleIoc.Default.GetInstance<SongAlbumsViewModel>();
        public PlayerControllerViewModel PlayerController => SimpleIoc.Default.GetInstance<PlayerControllerViewModel>();
    }
}