using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using IHECLibrary.Services;
using IHECLibrary.Services.Implementations;
using IHECLibrary.Tests;
using IHECLibrary.ViewModels;
using IHECLibrary.Views;
using Microsoft.Extensions.DependencyInjection;
using Supabase;
using System.IO;
using Serilog;
using Serilog.Debugging;
using Serilog.Sinks.Debug; // Added for Debug sink support
using System.Windows.Input;

namespace IHECLibrary
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;
        public static bool RunTests = false;

        public override void Initialize()
        {
            try
            {
                AvaloniaXamlLoader.Load(this);
            }
            catch (Exception ex)
            {
                DebugHelper.LogException(ex, "App Initialize");
                throw;
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            try
            {
                // Configuration de Serilog
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app.log");
                var logDirectory = Path.GetDirectoryName(logPath);
                if (logDirectory != null)
                {
                    Directory.CreateDirectory(logDirectory);
                }
                
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.Debug()
                    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                // Configuration des services
                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();

                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    try
                    {
                        // Get services
                        var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
                        var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
                        var mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
                        
                        DebugHelper.LogDebugInfo("Creating MainWindow");
                        // Create the main window - explicitly use Views.MainWindow
                        var mainWindow = new Views.MainWindow
                        {
                            DataContext = mainWindowViewModel
                        };
                        
                        DebugHelper.LogDebugInfo("Setting up navigation service");
                        // Set up navigation service
                        if (navigationService is NavigationService navService)
                        {
                            navService.SetMainWindow(mainWindow);
                        }
                        
                        // Assign the main window
                        desktop.MainWindow = mainWindow;
                        
                        // Log successful startup
                        Log.Information("Application started successfully");
                        DebugHelper.LogDebugInfo("Application started successfully");
                        
                        // Exécuter les tests si demandé
                        if (RunTests)
                        {
                            desktop.Startup += async (sender, e) =>
                            {
                                await TestManager.RunTests(_serviceProvider);
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log view creation errors with our specialized logging method
                        DebugHelper.LogViewCreationError(ex, "MainWindow");
                        
                        // Log any startup errors
                        Log.Fatal(ex, "Application failed to start: Error creating view");
                        
                        // Create a simple error window to show the error instead of crashing silently
                        CreateErrorWindow(ex, desktop);
                    }
                }

                base.OnFrameworkInitializationCompleted();
            }
            catch (Exception ex)
            {
                DebugHelper.LogException(ex, "OnFrameworkInitializationCompleted");
                
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    CreateErrorWindow(ex, desktop);
                }
                throw;
            }
        }

        private void CreateErrorWindow(Exception ex, IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                // Create a simple window to display the error
                var errorWindow = new Avalonia.Controls.Window
                {
                    Title = "Application Error",
                    Width = 600,
                    Height = 400
                };

                var stackPanel = new Avalonia.Controls.StackPanel
                {
                    Margin = new Avalonia.Thickness(20)
                };

                stackPanel.Children.Add(new Avalonia.Controls.TextBlock
                {
                    Text = "An error occurred while starting the application:",
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Margin = new Avalonia.Thickness(0, 0, 0, 10)
                });

                stackPanel.Children.Add(new Avalonia.Controls.TextBlock
                {
                    Text = ex.Message,
                    Margin = new Avalonia.Thickness(0, 0, 0, 10)
                });

                if (ex.InnerException != null)
                {
                    stackPanel.Children.Add(new Avalonia.Controls.TextBlock
                    {
                        Text = "Inner Exception:",
                        FontWeight = Avalonia.Media.FontWeight.Bold,
                        Margin = new Avalonia.Thickness(0, 10, 0, 5)
                    });

                    stackPanel.Children.Add(new Avalonia.Controls.TextBlock
                    {
                        Text = ex.InnerException.Message,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    });
                }

                var scrollViewer = new Avalonia.Controls.ScrollViewer
                {
                    Content = stackPanel
                };

                errorWindow.Content = scrollViewer;
                desktop.MainWindow = errorWindow;
            }
            catch (Exception errorWindowEx)
            {
                // If we can't even create the error window, log it
                DebugHelper.LogException(errorWindowEx, "Creating error window");
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register all services
            
            // First register Supabase client and authentication services
            // Supabase configuration
            var supabaseUrl = "https://kwsczjtdjexydcbzbpws.supabase.co";
            var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imt3c2N6anRkamV4eWRjYnpicHdzIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDUwNjkyNzMsImV4cCI6MjA2MDY0NTI3M30.xfwy8okepbA3d0yaDCUpUXUyvKYUKR1w7SLW3gam5HM";
            var supabaseOptions = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };
            var supabaseClient = new Client(supabaseUrl, supabaseKey, supabaseOptions);

            // Add Supabase client
            services.AddSingleton(supabaseClient);
            
            // Register navigation service first
            services.AddSingleton<INavigationService, NavigationService>();
            
            // Register auth service before viewmodels that depend on it
            services.AddSingleton<IAuthService>(provider => 
                new SupabaseAuthService(supabaseClient, provider.GetRequiredService<INavigationService>()));
            
            // Register other services
            services.AddSingleton<IUserService>(provider => 
                new SupabaseUserService(supabaseClient, provider.GetRequiredService<IAuthService>()));
            
            // Utiliser SupabaseBookService au lieu de MockBookService pour n'afficher que les livres de la base de données
            services.AddSingleton<IBookService>(provider => 
                new SupabaseBookService(supabaseClient, provider.GetRequiredService<IUserService>()));
            
            services.AddSingleton<IChatbotService>(provider => 
                new GeminiChatbotService("AIzaSyAHGzJNWYMGDDsSzpAUFn92XjETHFjQ07c", provider.GetRequiredService<IBookService>()));
            services.AddSingleton<IAdminService>(provider => 
                new SupabaseAdminService(supabaseClient, provider.GetRequiredService<IAuthService>()));
            
            // Now register all ViewModels with explicit dependencies
            services.AddTransient<LoginViewModel>(provider => new LoginViewModel(
                provider.GetRequiredService<INavigationService>(),
                provider.GetRequiredService<IAuthService>()
            ));
            
            services.AddTransient<RegisterViewModel>(provider => new RegisterViewModel(
                provider.GetRequiredService<INavigationService>(),
                provider.GetRequiredService<IAuthService>()
            ));
            
            services.AddTransient<HomeViewModel>(provider => new HomeViewModel(
                provider.GetRequiredService<INavigationService>(),
                provider.GetRequiredService<IBookService>(),
                provider.GetRequiredService<IUserService>()
            ));
            
            services.AddTransient<LibraryViewModel>(provider => new LibraryViewModel(
                provider.GetRequiredService<INavigationService>(),
                provider.GetRequiredService<IBookService>(),
                provider.GetRequiredService<IUserService>()
            ));
            
            // Check each remaining ViewModel and add explicit dependencies
            services.AddTransient<AdminLoginViewModel>(provider => new AdminLoginViewModel(
                provider.GetRequiredService<INavigationService>(),
                provider.GetRequiredService<IAuthService>()
            ));
            
            services.AddTransient<AdminRegisterViewModel>(provider => new AdminRegisterViewModel(
                provider.GetRequiredService<INavigationService>(),
                provider.GetRequiredService<IAuthService>()
            ));
            
            services.AddTransient<ProfileViewModel>(provider => new ProfileViewModel(
                provider.GetRequiredService<INavigationService>(),
                provider.GetRequiredService<IUserService>(),
                provider.GetRequiredService<IBookService>(),
                provider.GetRequiredService<IAuthService>()
            ));
            
            services.AddTransient<ChatbotViewModel>(provider => new ChatbotViewModel(
                provider.GetRequiredService<INavigationService>(),
                provider.GetRequiredService<IUserService>(),
                provider.GetRequiredService<IChatbotService>(),
                provider.GetRequiredService<IBookService>()
            ));
            
            services.AddTransient<AdminDashboardViewModel>(provider => new AdminDashboardViewModel(
                provider.GetRequiredService<INavigationService>(),
                provider.GetRequiredService<IAdminService>(),
                provider.GetRequiredService<IUserService>(),
                provider.GetRequiredService<IBookService>(),
                provider.GetRequiredService<IAuthService>()
            ));
            
            // Register MainWindowViewModel with explicit dependencies
            services.AddSingleton<MainWindowViewModel>(provider => new MainWindowViewModel(
                provider.GetRequiredService<INavigationService>(),
                provider.GetRequiredService<LoginViewModel>()
            ));
        }
    }
}
