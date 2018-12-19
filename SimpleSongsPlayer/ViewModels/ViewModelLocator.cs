using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using SimpleSongsPlayer.ViewModels.SideViews;

namespace SimpleSongsPlayer.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<FrameworkViewModel>();
            SimpleIoc.Default.Register<MusicInfoViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }

        public FrameworkViewModel Framework => SimpleIoc.Default.GetInstance<FrameworkViewModel>();
        public MusicListViewModel MusicList => new MusicListViewModel();
        public MusicGroupListViewModel MusicGroupList => new MusicGroupListViewModel();
        public MusicInfoViewModel MusicInfo => SimpleIoc.Default.GetInstance<MusicInfoViewModel>();
        public SettingsViewModel Settings => SimpleIoc.Default.GetInstance<SettingsViewModel>();
    }
}