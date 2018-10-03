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
            SimpleIoc.Default.Register<SongsViewModelLocator>();
            SimpleIoc.Default.Register<PlayerControllerViewModel>();
            SimpleIoc.Default.Register<PlayingViewModel>();
        }

        public FrameworkViewModel Framework => SimpleIoc.Default.GetInstance<FrameworkViewModel>();
        public AllSongListsViewModel AllSongLists => SimpleIoc.Default.GetInstance<AllSongListsViewModel>();
        public LoadingViewModel Loading => SimpleIoc.Default.GetInstance<LoadingViewModel>();
        public SongsViewModelLocator SongsLocator => SimpleIoc.Default.GetInstance<SongsViewModelLocator>();
        public PlayerControllerViewModel PlayerController => SimpleIoc.Default.GetInstance<PlayerControllerViewModel>();
        public PlayingViewModel Playing => SimpleIoc.Default.GetInstance<PlayingViewModel>();
    }
}