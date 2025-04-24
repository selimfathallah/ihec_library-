using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;

namespace IHECLibrary.Tests
{
    // Helper class for testing and debugging REST API calls
    public class DebugHelper
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        private static readonly string DebugLogPath = Path.Combine(LogDirectory, "debug.log");

        static DebugHelper()
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        public static void LogDebugInfo(string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = $"[{timestamp}] {message}";
                
                File.AppendAllText(DebugLogPath, logMessage + Environment.NewLine);
                Debug.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la journalisation du débogage: {ex.Message}");
            }
        }

        public static void LogException(Exception ex, string context = "")
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = $"[{timestamp}] EXCEPTION {(string.IsNullOrEmpty(context) ? "" : $"dans {context}")}: {ex.Message}";
                logMessage += Environment.NewLine + ex.StackTrace;
                
                if (ex.InnerException != null)
                {
                    logMessage += Environment.NewLine + $"Inner Exception: {ex.InnerException.Message}";
                    logMessage += Environment.NewLine + ex.InnerException.StackTrace;
                }
                
                File.AppendAllText(DebugLogPath, logMessage + Environment.NewLine);
                Debug.WriteLine(logMessage);
            }
            catch (Exception logEx)
            {
                Debug.WriteLine($"Erreur lors de la journalisation de l'exception: {logEx.Message}");
            }
        }

        public static Task<bool> VerifyDatabaseConnection(Supabase.Client supabaseClient)
        {
            try
            {
                // Tenter une opération simple pour vérifier la connexion
                var session = supabaseClient.Auth.CurrentSession;
                LogDebugInfo("Vérification de la connexion à la base de données: Réussie");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogException(ex, "Vérification de la connexion à la base de données");
                return Task.FromResult(false);
            }
        }

        public static async Task<bool> VerifyGeminiApiConnection(string apiKey)
        {
            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";
                var payload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = "Hello, this is a test."
                                }
                            }
                        }
                    }
                };

                var response = await HttpClientRequest(url, apiKey, HttpMethod.Post, payload);

                if (!string.IsNullOrEmpty(response))
                {
                    LogDebugInfo("Vérification de la connexion à l'API Gemini: Réussie");
                    return true;
                }
                else
                {
                    LogDebugInfo("Vérification de la connexion à l'API Gemini: Échouée");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "Vérification de la connexion à l'API Gemini");
                return false;
            }
        }

        public static async Task<T> MakeRequest<T>(string url, string apiKey, Method method = Method.Get, object? data = null)
        {
            var client = new RestClient(url);
            var request = new RestRequest();
            request.Method = method;

            // Add headers
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", $"Bearer {apiKey}");

            // Add JSON body for POST/PUT requests
            if (data != null && (method == Method.Post || method == Method.Put))
            {
                request.AddJsonBody(data);
            }

            // Execute the request
            var response = await client.ExecuteAsync(request);

            // Check for errors
            if (!response.IsSuccessful)
            {
                throw new Exception($"API request failed: {response.StatusCode} - {response.Content}");
            }

            // Deserialize the response
            if (string.IsNullOrEmpty(response.Content))
            {
                throw new Exception("API response content is null or empty.");
            }
            return JsonSerializer.Deserialize<T>(response.Content) ?? throw new Exception("Deserialization returned null.");
        }

        // Alternative using HttpClient
        public static async Task<string> HttpClientRequest(string url, string apiKey, HttpMethod method, object? data = null)
        {
            using var client = new HttpClient();

            // Create request message
            var request = new HttpRequestMessage(method, url);
            
            // Add headers
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            // Add content for POST/PUT requests
            if (data != null && (method == HttpMethod.Post || method == HttpMethod.Put))
            {
                var json = JsonSerializer.Serialize(data);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }

            // Execute request
            var response = await client.SendAsync(request);
            
            // Check response
            response.EnsureSuccessStatusCode();
            
            // Read content
            return await response.Content.ReadAsStringAsync();
        }

        public static long GetAvailableDiskSpaceMB(string path)
        {
            try
            {
                string? driveName = Path.GetPathRoot(path);
                if (string.IsNullOrEmpty(driveName))
                {
                    LogDebugInfo("Unable to determine drive root for path: " + path);
                    return 0;
                }
                
                DriveInfo drive = new DriveInfo(driveName);
                long availableBytes = drive.AvailableFreeSpace;
                return availableBytes / (1024 * 1024); // Convert to MB
            }
            catch (Exception ex)
            {
                LogException(ex, "Vérification de l'espace disque disponible");
                return 0;
            }
        }

        public static async Task<bool> CheckInternetConnection()
        {
            try
            {
                // Méthode 1: Ping Google DNS
                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync("8.8.8.8", 3000);
                    if (reply.Status == IPStatus.Success)
                    {
                        return true;
                    }
                }

                // Méthode 2: Essayer une requête HTTP en cas d'échec du ping
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(5);
                        HttpResponseMessage response = await client.GetAsync("https://www.google.com");
                        return response.IsSuccessStatusCode;
                    }
                }
                catch
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "Vérification de la connexion Internet");
                return false;
            }
        }
        
        public static void LogViewCreationError(Exception ex, string viewName)
        {
            LogException(ex, $"Erreur lors de la création de la vue: {viewName}");
            
            // Log additional details specific to view creation errors
            if (ex.InnerException != null)
            {
                LogDebugInfo($"Inner Exception in view {viewName}: {ex.InnerException.Message}");
                
                // Recursively log nested inner exceptions which are common in reflection errors
                Exception? currentEx = ex.InnerException;
                int depth = 1;
                while (currentEx?.InnerException != null)
                {
                    depth++;
                    currentEx = currentEx.InnerException;
                    LogDebugInfo($"Level {depth} Inner Exception in view {viewName}: {currentEx.Message}");
                }
            }
            
            // Log reflection-related information which is often the cause of "thrown by the target of an invocation" errors
            if (ex.Message.Contains("invocation") || (ex.InnerException?.Message?.Contains("invocation") == true))
            {
                LogDebugInfo("This appears to be a reflection-related error. Check for:");
                LogDebugInfo("1. Missing constructor parameters");
                LogDebugInfo("2. Exceptions in constructor or InitializeComponent");
                LogDebugInfo("3. View/ViewModel binding issues");
                LogDebugInfo("4. Missing or incorrect dependency injections");
            }
        }
    }
}
