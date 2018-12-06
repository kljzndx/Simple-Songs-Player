using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;

namespace SimpleSongsPlayer.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<FrameworkViewModel>();
        }

        public FrameworkViewModel Framework => SimpleIoc.Default.GetInstance<FrameworkViewModel>();
        public MusicListViewModel MusicList => new MusicListViewModel();
        public MusicGroupListViewModel MusicGroupList => new MusicGroupListViewModel();
    }
}