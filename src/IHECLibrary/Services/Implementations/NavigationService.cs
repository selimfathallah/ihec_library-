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
            Console.WriteLine($"NavigateToAsync called: {viewName}");
            
            if (_mainWindow == null)
            {
                Console.WriteLine("MainWindow is null, cannot navigate");
                throw new InvalidOperationException("MainWindow n'a pas été initialisé.");
            }

            try
            {
                Console.WriteLine($"Resolving ViewModel for: {viewName}");
                
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
                    "Chatbot" => _serviceProvider.GetRequiredService<ChatbotViewModel>(),
                    "AdminDashboard" => _serviceProvider.GetRequiredService<AdminDashboardViewModel>(),
                    "EditProfile" => _serviceProvider.GetRequiredService<EditProfileViewModel>(), // Added EditProfile
                    _ => throw new ArgumentException($"Vue non reconnue: {viewName}")
                };

                Console.WriteLine($"ViewModel resolved: {viewModel.GetType().Name}");

                // Passer le paramètre au ViewModel si nécessaire
                if (parameter != null && viewModel is IParameterizedViewModel parameterizedViewModel)
                {
                    Console.WriteLine("Initializing parameterized ViewModel");
                    await parameterizedViewModel.InitializeAsync(parameter);
                }

                // Ensure we're on the UI thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Console.WriteLine("Raising NavigationRequested event");
                    // Raise the event for the MainWindowViewModel
                    NavigationRequested?.Invoke(this, new NavigationEventArgs(viewModel));
                    
                    // Mettre à jour le DataContext de la fenêtre principale si la fenêtre n'utilise pas le MainWindowViewModel
                    if (_mainWindow.DataContext is not MainWindowViewModel)
                    {
                        Console.WriteLine("Setting DataContext directly on MainWindow");
                        _mainWindow.DataContext = viewModel;
                    }
                    else
                    {
                        Console.WriteLine("MainWindow is using MainWindowViewModel");
                    }
                    
                    Console.WriteLine($"Navigation to {viewName} completed");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }

    // Interface pour les ViewModels qui acceptent un paramètre lors de la navigation
    public interface IParameterizedViewModel
    {
        Task InitializeAsync(object parameter);
    }
}
