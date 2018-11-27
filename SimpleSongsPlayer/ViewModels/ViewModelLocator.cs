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
            SimpleIoc.Default.Register<SongListViewModel>();
        }

        public FrameworkViewModel Framework => SimpleIoc.Default.GetInstance<FrameworkViewModel>();
        public SongListViewModel SongList => SimpleIoc.Default.GetInstance<SongListViewModel>();

    }
}