using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using IHECLibrary.Tests;

namespace IHECLibrary
{
    public class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                // Vérifier si l'argument de test est présent
                if (args.Length > 0 && args[0].ToLower() == "--test")
                {
                    App.RunTests = true;
                }

                // Configurer le répertoire de logs
                var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // Journaliser le démarrage de l'application
                DebugHelper.LogDebugInfo("Application IHEC Library démarrée");

                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                // Journaliser toute exception non gérée
                File.WriteAllText(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"),
                    $"Date: {DateTime.Now}\nException: {ex.Message}\nStack Trace: {ex.StackTrace}"
                );
                throw;
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
