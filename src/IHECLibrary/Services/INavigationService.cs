using System;
using System.Threading.Tasks;
using IHECLibrary.ViewModels;
using IHECLibrary.Views;

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
        
        void SetMainWindow(Views.MainWindow mainWindow);
        
        Task NavigateToAsync(string viewName, object? parameter = null);
        
        ViewModelBase GetInitialViewModel();
    }
}
