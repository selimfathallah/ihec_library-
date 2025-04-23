using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IHECLibrary.Services;
using IHECLibrary.Services.Implementations;
using Supabase;

namespace IHECLibrary.Tests
{
    public class ApplicationTester
    {
        private readonly string _testReportPath;
        private readonly List<string> _testResults = new List<string>();
        private readonly Supabase.Client _supabaseClient;
        private readonly string _geminiApiKey;

        public ApplicationTester(Supabase.Client supabaseClient, string geminiApiKey)
        {
            _supabaseClient = supabaseClient;
            _geminiApiKey = geminiApiKey;
            _testReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_report.html");
        }

        public async Task RunSystemTests()
        {
            AddTestResult("h1", "Rapport de test de l'application IHEC Library");
            AddTestResult("p", $"Date et heure: {DateTime.Now}");

            bool allPassed = true;
            try
            {
                await TestSystemRequirements();
                bool apiPassed = await TestApiConnections();
                allPassed = allPassed && apiPassed;
            }
            catch (Exception ex)
            {
                allPassed = false;
                AddTestResult("h2", "Erreur inattendue pendant les tests");
                AddTestResult("p", $"Une erreur s'est produite : {ex.Message}");
                AddTestResult("pre", ex.StackTrace ?? "Pas de trace de la pile disponible.");
                DebugHelper.LogDebugInfo($"Erreur inattendue pendant RunSystemTests: {ex}");
            }
            finally
            {
                AddTestResult("h2", "Résumé");
                AddTestResult("p", allPassed ? "Tous les tests ont réussi." : "Des erreurs ont été détectées lors des tests.");
                GenerateHtmlReport();
            }
        }

        private async Task TestSystemRequirements()
        {
            AddTestResult("h2", "1. Vérification des exigences système");
            
            // Vérification du système d'exploitation
            string osVersion = Environment.OSVersion.ToString();
            bool osSupported = osVersion.Contains("Windows") || osVersion.Contains("Unix") || osVersion.Contains("Linux");
            AddTestResult("test", "Système d'exploitation", osVersion, osSupported);
            
            // Vérification de la version de .NET
            string dotNetVersion = Environment.Version.ToString();
            bool dotNetSupported = Environment.Version.Major >= 6;
            AddTestResult("test", "Version .NET", dotNetVersion, dotNetSupported);
            
            // Vérification de l'espace disque
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            long freeSpaceMB = DebugHelper.GetAvailableDiskSpaceMB(appDirectory);
            bool diskSpaceOk = freeSpaceMB > 500; // Exiger au moins 500 Mo d'espace libre
            AddTestResult("test", "Espace disque disponible", $"{freeSpaceMB} Mo", diskSpaceOk);
            
            // Vérification de la connexion Internet
            bool internetConnected = await DebugHelper.CheckInternetConnection();
            AddTestResult("test", "Connexion Internet", internetConnected ? "Disponible" : "Non disponible", internetConnected);
            
            await Task.CompletedTask;
        }

        private async Task<bool> TestApiConnections()
        {
            AddTestResult("h2", "2. Vérification des connexions API");
            bool allPassed = true;

            // Tester la connexion à Supabase
            bool supabaseConnected = false;
            if (_supabaseClient != null)
            {
                supabaseConnected = await DebugHelper.VerifyDatabaseConnection(_supabaseClient);
                AddTestResult("test", "Connexion à Supabase", supabaseConnected ? "Réussie" : "Échec", supabaseConnected);
            }
            else
            {
                AddTestResult("test", "Connexion à Supabase", "Client non configuré", false);
                allPassed = false;
            }
            allPassed = allPassed && supabaseConnected;

            // Tester la connexion à l'API Gemini
            bool geminiConnected = false;
            if (!string.IsNullOrWhiteSpace(_geminiApiKey))
            {
                geminiConnected = await DebugHelper.VerifyGeminiApiConnection(_geminiApiKey);
                AddTestResult("test", "Connexion à l'API Gemini", geminiConnected ? "Réussie" : "Échec", geminiConnected);
            }
            else
            {
                AddTestResult("test", "Connexion à l'API Gemini", "Clé API non configurée", false);
                allPassed = false;
            }
            allPassed = allPassed && geminiConnected;

            return allPassed;
        }

        private void AddTestResult(string type, string content)
        {
            _testResults.Add($"<{type}>{content}</{type}>");
        }

        private void AddTestResult(string type, string label, string value, bool success)
        {
            string resultClass = success ? "success" : "failure";
            string resultIcon = success ? "✓" : "✗";
            
            _testResults.Add($@"<div class=""test-result {resultClass}"">
                <span class=""test-icon"">{resultIcon}</span>
                <span class=""test-label"">{label}:</span>
                <span class=""test-value"">{value}</span>
            </div>");
        }

        private void GenerateHtmlReport()
        {
            string htmlContent = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>Rapport de test - IHEC Library</title>
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
    </style>
</head>
<body>
    {string.Join("\n    ", _testResults)}
    
    <p class=""timestamp"">Rapport généré le {DateTime.Now}</p>
</body>
</html>";

            File.WriteAllText(_testReportPath, htmlContent);
            DebugHelper.LogDebugInfo($"Rapport de test généré: {_testReportPath}");
        }
    }
}
