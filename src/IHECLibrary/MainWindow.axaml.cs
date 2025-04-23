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
            // Récupérer le service de navigation et définir cette fenêtre comme fenêtre principale
            var applicationLifetime = App.Current?.ApplicationLifetime;
            if (applicationLifetime != null)
            {
                var services = applicationLifetime.GetType().GetProperty("Services")?.GetValue(applicationLifetime) as IServiceProvider;
                if (services != null)
                {
                    var navService = services.GetService<INavigationService>() as NavigationService;
                    navService?.SetMainWindow(this);
                }
            }
        }
    }
}
