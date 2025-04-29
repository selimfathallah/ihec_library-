using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using IHECLibrary.ViewModels;
using Avalonia.Controls;
using IHECLibrary.Views;

namespace IHECLibrary.Services.Implementations
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private Views.MainWindow? _mainWindow;
        
        public event EventHandler<NavigationEventArgs>? NavigationRequested;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void SetMainWindow(Views.MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }
        
        public ViewModelBase GetInitialViewModel()
        {
            // Default to login view or whatever should be shown initially
            return _serviceProvider.GetRequiredService<LoginViewModel>();
        }

        public async Task NavigateToAsync(string viewName, object? parameter = null)
        {
            if (_mainWindow == null)
                throw new InvalidOperationException("MainWindow n'a pas été initialisé.");

            // Déterminer le ViewModel à utiliser en fonction du nom de la vue
            ViewModelBase viewModel = viewName switch
            {
                "Login" => _serviceProvider.GetRequiredService<LoginViewModel>(),
                "Register" => _serviceProvider.GetRequiredService<RegisterViewModel>(),
                "AdminLogin" => _serviceProvider.GetRequiredService<AdminLoginViewModel>(),
                "AdminRegister" => _serviceProvider.GetRequiredService<AdminRegisterViewModel>(),
                "Home" => _serviceProvider.GetRequiredService<HomeViewModel>(),
                "Library" => _serviceProvider.GetRequiredService<LibraryViewModel>(),
                "Profile" => _serviceProvider.GetRequiredService<ProfileViewModel>(),
                "EditProfile" => _serviceProvider.GetRequiredService<EditProfileViewModel>(),
                "Chatbot" => _serviceProvider.GetRequiredService<ChatbotViewModel>(),
                "AdminDashboard" => _serviceProvider.GetRequiredService<AdminDashboardViewModel>(),
                _ => throw new ArgumentException($"Vue non reconnue: {viewName}")
            };

            // Special case: If we're navigating to the Profile view, ensure it reloads data
            // This is especially important when coming back from EditProfile
            if (viewName == "Profile" && viewModel is ProfileViewModel profileViewModel)
            {
                System.Diagnostics.Debug.WriteLine("NavigationService: Navigating to Profile, forcing data refresh");
                profileViewModel.RefreshData();
            }

            // Passer le paramètre au ViewModel si nécessaire
            if (parameter != null && viewModel is IParameterizedViewModel parameterizedViewModel)
            {
                await parameterizedViewModel.InitializeAsync(parameter);
            }

            // Raise the event for the MainWindowViewModel
            NavigationRequested?.Invoke(this, new NavigationEventArgs(viewModel));
            
            // Mettre à jour le DataContext de la fenêtre principale si la fenêtre n'utilise pas le MainWindowViewModel
            if (_mainWindow.DataContext is not MainWindowViewModel)
            {
                _mainWindow.DataContext = viewModel;
            }
        }
    }

    // Interface pour les ViewModels qui acceptent un paramètre lors de la navigation
    public interface IParameterizedViewModel
    {
        Task InitializeAsync(object parameter);
    }
}
