using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace IHECLibrary.Tests
{
    public static class TestLauncher
    {
        public static async Task LaunchTests(IServiceProvider serviceProvider, int iterations = 1)
        {
            try
            {
                Console.WriteLine("=== Lancement des tests de l'application IHEC Library ===");
                
                if (iterations > 1)
                {
                    Console.WriteLine($"Mode itératif activé: {iterations} itérations");
                }
                
                // Exécuter les tests système
                Console.WriteLine("1. Exécution des tests système...");
                var supabaseClient = serviceProvider.GetRequiredService<Supabase.Client>();
                var geminiApiKey = "AIzaSyAHGzJNWYMGDDsSzpAUFn92XjETHFjQ07c";
                
                var applicationTester = new ApplicationTester(supabaseClient, geminiApiKey);
                await applicationTester.RunSystemTests();
                
                // Exécuter les tests fonctionnels avec itérations
                Console.WriteLine("2. Exécution des tests fonctionnels...");
                await TestManager.RunTests(serviceProvider, iterations);
                
                // Générer un rapport de test consolidé
                GenerateConsolidatedReport(iterations > 1);
                
                Console.WriteLine("=== Tests terminés ===");
                Console.WriteLine($"Rapports disponibles dans: {Path.Combine(AppDomain.CurrentDomain.BaseDirectory)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du lancement des tests: {ex.Message}");
                DebugHelper.LogException(ex, "TestLauncher");
            }
        }
        
        private static void GenerateConsolidatedReport(bool isIterative = false)
        {
            try
            {
                var reportPrefix = isIterative ? "iterative_" : "";
                var reportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{reportPrefix}test_consolidated_report.html");
                var systemReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_report.html");
                var functionalReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{reportPrefix}test_results.log");
                
                string systemReportContent = File.Exists(systemReportPath) ? File.ReadAllText(systemReportPath) : "Rapport système non disponible";
                string functionalReportContent = File.Exists(functionalReportPath) ? File.ReadAllText(functionalReportPath) : "Rapport fonctionnel non disponible";
                
                // Extraire le contenu du body du rapport système
                string systemBodyContent = "";
                if (systemReportContent.Contains("<body>") && systemReportContent.Contains("</body>"))
                {
                    int startIndex = systemReportContent.IndexOf("<body>") + 6;
                    int endIndex = systemReportContent.IndexOf("</body>");
                    systemBodyContent = systemReportContent.Substring(startIndex, endIndex - startIndex);
                }
                else
                {
                    systemBodyContent = systemReportContent;
                }
                
                string htmlContent = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>Rapport de test consolidé - IHEC Library</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        h1 {{ color: #2E74A8; }}
        h2 {{ color: #2E74A8; margin-top: 30px; }}
        .test-result {{ margin: 10px 0; padding: 10px; border-radius: 5px; }}
        .success {{ background-color: #E6FFE6; }}
        .failure {{ background-color: #FFE6E6; }}
        .test-icon {{ font-weight: bold; margin-right: 10px; }}
        .test-label {{ font-weight: bold; margin-right: 10px; }}
        .test-value {{ }}
        .timestamp {{ color: #666; font-style: italic; margin-top: 30px; }}
        .functional-report {{ background-color: #F8F9FA; padding: 15px; border-radius: 5px; white-space: pre-wrap; }}
        .iterative {{ background-color: #E6F7FF; border-left: 4px solid #1890FF; padding-left: 15px; }}
    </style>
</head>
<body>
    <h1>Rapport de test consolidé - IHEC Library {(isIterative ? "(Mode Itératif)" : "")}</h1>
    <p>Date et heure: {DateTime.Now}</p>
    
    <h2>Tests système</h2>
    {systemBodyContent}
    
    <h2>Tests fonctionnels{(isIterative ? " (Itératifs)" : "")}</h2>
    <div class=""functional-report{(isIterative ? " iterative" : "")}"">
{functionalReportContent}
    </div>
    
    <p class=""timestamp"">Rapport consolidé généré le {DateTime.Now}</p>
</body>
</html>";

                File.WriteAllText(reportPath, htmlContent);
                DebugHelper.LogDebugInfo($"Rapport consolidé généré: {reportPath}");
            }
            catch (Exception ex)
            {
                DebugHelper.LogException(ex, "Génération du rapport consolidé");
            }
        }
        
        public static async Task ContinueToIterate(IServiceProvider serviceProvider, int iterations = 3)
        {
            if (iterations <= 0)
            {
                Console.WriteLine("Le nombre d'itérations doit être supérieur à 0");
                return;
            }
            
            Console.WriteLine($"=== Lancement des tests itératifs ({iterations} itérations) ===");
            await LaunchTests(serviceProvider, iterations);
        }
    }
}
