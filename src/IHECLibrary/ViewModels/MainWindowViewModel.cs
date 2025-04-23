using CommunityToolkit.Mvvm.ComponentModel;
using IHECLibrary.Services;
using System;

namespace IHECLibrary.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentViewModel;
        
        [ObservableProperty]
        private string _greeting = "Bienvenue à la bibliothèque IHEC";

        private readonly INavigationService _navigationService;

        public MainWindowViewModel(INavigationService navigationService, LoginViewModel initialViewModel)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            // Set initial view model explicitly
            CurrentViewModel = initialViewModel ?? throw new ArgumentNullException(nameof(initialViewModel));
            // Subscribe to navigation events
            _navigationService.NavigationRequested += OnNavigationRequested;
        }

        private void OnNavigationRequested(object? sender, NavigationEventArgs e)
        {
            if (e?.TargetViewModel != null)
            {
                CurrentViewModel = e.TargetViewModel;
            }
        }
    }
}
