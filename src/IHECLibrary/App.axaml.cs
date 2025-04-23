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

namespace IHECLibrary
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;
        public static bool RunTests = false;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
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
                    
                    // Create the main window
                    var mainWindow = new MainWindow
                    {
                        DataContext = mainWindowViewModel
                    };
                    
                    // Set up navigation service
                    if (navigationService is NavigationService navService)
                    {
                        navService.SetMainWindow(mainWindow);
                    }
                    
                    // Assign the main window
                    desktop.MainWindow = mainWindow;
                    
                    // Log successful startup
                    Log.Information("Application started successfully");
                    
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
                    // Log any startup errors
                    Log.Fatal(ex, "Application failed to start");
                    throw;
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register all services
            
            // First register all ViewModels that don't depend on MainWindowViewModel
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<AdminLoginViewModel>();
            services.AddTransient<AdminRegisterViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<LibraryViewModel>();
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<ChatbotViewModel>();
            services.AddTransient<AdminDashboardViewModel>();
            
            // Register services
            services.AddSingleton<INavigationService, NavigationService>();
            
            // Register MainWindowViewModel with explicit dependencies
            services.AddSingleton<MainWindowViewModel>(provider => new MainWindowViewModel(
                provider.GetRequiredService<INavigationService>(),
                provider.GetRequiredService<LoginViewModel>()
            ));
            
            // Other service registrations
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
            
            // Register other services
            services.AddSingleton<IAuthService>(provider => new SupabaseAuthService(supabaseClient, provider.GetRequiredService<INavigationService>()));
            services.AddSingleton<IUserService>(provider => new SupabaseUserService(supabaseClient, provider.GetRequiredService<IAuthService>()));
            services.AddSingleton<IBookService>(provider => new SupabaseBookService(supabaseClient, provider.GetRequiredService<IUserService>()));
            services.AddSingleton<IChatbotService>(provider => new GeminiChatbotService("AIzaSyAHGzJNWYMGDDsSzpAUFn92XjETHFjQ07c", provider.GetRequiredService<IBookService>()));
            services.AddSingleton<IAdminService>(provider => new SupabaseAdminService(supabaseClient, provider.GetRequiredService<IAuthService>()));
        }
    }
}
