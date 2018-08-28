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
            SimpleIoc.Default.Register<AllSongsViewModel>();
            SimpleIoc.Default.Register<AllSongArtistsViewModel>();
            SimpleIoc.Default.Register<AllSongAlbumsViewModel>();
        }

        public MainViewModel Main => SimpleIoc.Default.GetInstance<MainViewModel>();
        public LoadingViewModel Loading => SimpleIoc.Default.GetInstance<LoadingViewModel>();
        public AllSongsViewModel AllSongs => SimpleIoc.Default.GetInstance<AllSongsViewModel>();
        public AllSongArtistsViewModel AllSongArtists => SimpleIoc.Default.GetInstance<AllSongArtistsViewModel>();
        public AllSongAlbumsViewModel AllSongAlbums => SimpleIoc.Default.GetInstance<AllSongAlbumsViewModel>();
    }
}