using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using IHECLibrary.Services;
using IHECLibrary.Services.Implementations;
using IHECLibrary.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace IHECLibrary
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            
            // Initialiser le service de navigation
            Loaded += MainWindow_Loaded;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("MainWindow loaded, initializing navigation service");
                
                // Récupérer le service de navigation et définir cette fenêtre comme fenêtre principale
                var applicationLifetime = App.Current?.ApplicationLifetime;
                if (applicationLifetime != null)
                {
                    Console.WriteLine("Application lifetime is available");
                    var services = applicationLifetime.GetType().GetProperty("Services")?.GetValue(applicationLifetime) as IServiceProvider;
                    if (services != null)
                    {
                        Console.WriteLine("Service provider is available");
                        var navService = services.GetService<INavigationService>() as NavigationService;
                        if (navService != null)
                        {
                            Console.WriteLine("Navigation service found, setting main window");
                            navService.SetMainWindow(this);
                            Console.WriteLine("Main window set on navigation service");
                        }
                        else
                        {
                            Console.WriteLine("ERROR: Navigation service is null");
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Service provider is null");
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: Application lifetime is null");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR initializing navigation service: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
