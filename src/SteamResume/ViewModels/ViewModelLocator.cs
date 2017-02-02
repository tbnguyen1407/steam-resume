using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using SteamResume.Repositories;

namespace SteamResume.ViewModels
{
    class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<IPlayerRepository, PlayerRepository>();
        }

        public MainViewModel MainVM { get { return ServiceLocator.Current.GetInstance<MainViewModel>(); } }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
