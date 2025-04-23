using System;
using System.Threading.Tasks;
using IHECLibrary.ViewModels;

namespace IHECLibrary.Services
{
    public class NavigationEventArgs : EventArgs
    {
        public ViewModelBase TargetViewModel { get; }
        
        public NavigationEventArgs(ViewModelBase targetViewModel)
        {
            TargetViewModel = targetViewModel;
        }
    }

    public interface INavigationService
    {
        event EventHandler<NavigationEventArgs>? NavigationRequested;
        
        Task NavigateToAsync(string viewName, object? parameter = null);
        
        ViewModelBase GetInitialViewModel();
    }
}
