using Google.Apis.Auth.OAuth2;
using RestSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IHECLibrary.Services.Implementations
{
    public class GeminiChatbotService : IChatbotService
    {
        private readonly string _apiKey;
        private readonly IBookService _bookService;
        private readonly string _geminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

        public GeminiChatbotService(string apiKey, IBookService bookService)
        {
            _apiKey = apiKey;
            _bookService = bookService;
        }

        public async Task<ChatbotResponse> GetResponseAsync(string userMessage)
        {
            try
            {
                var client = new RestClient(_geminiApiUrl + $"?key={_apiKey}");
                var request = new RestRequest("", Method.Post);
                request.AddHeader("Content-Type", "application/json");

                // Construire le prompt avec le contexte de la bibliothèque
                var prompt = $@"Tu es HEC 1.0, l'assistant virtuel de la bibliothèque IHEC Carthage. 
Tu aides les étudiants à trouver des livres, à obtenir des informations sur la bibliothèque, 
et à répondre à leurs questions académiques. Réponds en français de manière concise et utile.

Question de l'utilisateur: {userMessage}";

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
                                    text = prompt
                                }
                            }
                        }
                    }
                };

                request.AddJsonBody(JsonConvert.SerializeObject(payload));
                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    // Add null check for response content before deserializing
                    if (string.IsNullOrEmpty(response.Content))
                    {
                        Console.WriteLine("Error: Empty response content from Gemini API in GetResponseAsync.");
                        return new ChatbotResponse
                        {
                            Message = "Je suis désolé, la réponse du service est vide. Veuillez réessayer.",
                            Suggestions = GetDefaultSuggestions()
                        };
                    }

                    var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(response.Content);
                    // Add null check for the deserialized object
                    if (geminiResponse == null)
                    {
                        Console.WriteLine("Error: Failed to deserialize Gemini response in GetResponseAsync."); // Or use a proper logger
                        return new ChatbotResponse
                        {
                            Message = "Je suis désolé, la réponse du service est invalide. Veuillez réessayer.",
                            Suggestions = GetDefaultSuggestions() // Use a helper for default suggestions
                        };
                    }

                    // Use FirstOrDefault for safer access
                    var responseText = geminiResponse.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text
                                       ?? "Je suis désolé, je n'ai pas pu traiter votre demande.";

                    var chatbotResponse = new ChatbotResponse
                    {
                        Message = responseText
                    };

                    // Ajouter des suggestions en fonction du message de l'utilisateur
                    chatbotResponse.Suggestions = GetSuggestionsForUserMessage(userMessage);

                    // Ajouter des recommandations de livres si la question concerne des livres
                    if (userMessage.ToLower().Contains("livre") || userMessage.ToLower().Contains("recommand"))
                    {
                        var bookRecommendations = await GetBookRecommendationsAsync(userMessage);
                        if (bookRecommendations.Count > 0)
                        {
                            chatbotResponse.BookRecommendations = bookRecommendations;
                        }
                    }

                    return chatbotResponse;
                }
                else
                {
                    // Log the error details from the response if available
                    Console.WriteLine($"Error from Gemini API in GetResponseAsync: {response.StatusCode} - {response.ErrorMessage}");
                    return new ChatbotResponse
                    {
                        Message = "Je suis désolé, je rencontre des difficultés techniques. Veuillez réessayer plus tard.",
                        Suggestions = GetDefaultSuggestions()
                    };
                }
            }
            catch (JsonException jsonEx) // Catch specific JSON errors
            {
                Console.WriteLine($"JSON Deserialization Error in GetResponseAsync: {jsonEx.Message}");
                return new ChatbotResponse
                {
                    Message = "Je suis désolé, une erreur s'est produite lors du traitement de la réponse. Veuillez réessayer.",
                    Suggestions = GetDefaultSuggestions()
                };
            }
            catch (Exception ex) // General catch block
            {
                Console.WriteLine($"Error in GetResponseAsync: {ex.Message}"); // Log the exception
                return new ChatbotResponse
                {
                    Message = "Je suis désolé, une erreur s'est produite. Veuillez réessayer plus tard.",
                    Suggestions = GetDefaultSuggestions()
                };
            }
        }

        public async Task<List<BookModel>> GetBookRecommendationsAsync(string query)
        {
            try
            {
                // Extraire les mots-clés pertinents de la requête
                var keywords = ExtractKeywords(query);
                
                // Rechercher des livres en fonction des mots-clés
                List<BookModel> books = new List<BookModel>();
                foreach (var keyword in keywords)
                {
                    var results = await _bookService.GetBooksBySearchAsync(keyword);
                    foreach (var book in results)
                    {
                        if (!books.Any(b => b.Id == book.Id))
                        {
                            books.Add(book);
                        }
                    }
                    
                    // Limiter à 3 livres maximum
                    if (books.Count >= 3)
                    {
                        books = books.Take(3).ToList();
                        break;
                    }
                }
                
                return books;
            }
            catch
            {
                return new List<BookModel>();
            }
        }

        public async Task<string> GetResearchAssistanceAsync(string topic)
        {
            try
            {
                var client = new RestClient(_geminiApiUrl + $"?key={_apiKey}");
                var request = new RestRequest("", Method.Post);
                request.AddHeader("Content-Type", "application/json");

                var prompt = $@"Tu es HEC 1.0, l'assistant de recherche de la bibliothèque IHEC Carthage. 
Un étudiant te demande de l'aide pour sa recherche sur le sujet suivant : '{topic}'.
Fournis des conseils méthodologiques, des suggestions de sources académiques, et des étapes 
pour mener à bien cette recherche. Réponds en français de manière structurée et utile.";

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
                                    text = prompt
                                }
                            }
                        }
                    }
                };

                request.AddJsonBody(JsonConvert.SerializeObject(payload));
                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    // Add null check for response content before deserializing
                    if (string.IsNullOrEmpty(response.Content))
                    {
                        Console.WriteLine("Error: Empty response content from Gemini API in GetResearchAssistanceAsync.");
                        return "Je suis désolé, la réponse du service est vide. Veuillez réessayer.";
                    }

                    var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(response.Content);
                    // Add null check and use FirstOrDefault for safer access
                    if (geminiResponse == null)
                    {
                         Console.WriteLine("Error: Failed to deserialize Gemini response in GetResearchAssistanceAsync.");
                         return "Je suis désolé, la réponse du service est invalide. Veuillez réessayer.";
                    }
                    return geminiResponse.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text
                           ?? "Je suis désolé, je n'ai pas pu traiter votre demande.";
                }
                else
                {
                    Console.WriteLine($"Error from Gemini API in GetResearchAssistanceAsync: {response.StatusCode} - {response.ErrorMessage}");
                    return "Je suis désolé, je rencontre des difficultés techniques. Veuillez réessayer plus tard.";
                }
            }
            catch (JsonException jsonEx) // Catch specific JSON errors
            {
                Console.WriteLine($"JSON Deserialization Error in GetResearchAssistanceAsync: {jsonEx.Message}");
                return "Je suis désolé, une erreur s'est produite lors du traitement de la réponse. Veuillez réessayer.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetResearchAssistanceAsync: {ex.Message}");
                return "Je suis désolé, une erreur s'est produite. Veuillez réessayer plus tard.";
            }
        }

        public async Task<string> GetLibraryInformationAsync(string query)
        {
            try
            {
                var client = new RestClient(_geminiApiUrl + $"?key={_apiKey}");
                var request = new RestRequest("", Method.Post);
                request.AddHeader("Content-Type", "application/json");

                // Informations de base sur la bibliothèque
                var libraryInfo = @"
Horaires d'ouverture de la bibliothèque IHEC Carthage :
- Lundi au vendredi : 8h00 à 18h00
- Samedi : 9h00 à 13h00
- Fermée le dimanche

Règles de la bibliothèque :
- Carte d'étudiant obligatoire pour emprunter des livres
- Maximum 3 livres empruntés simultanément
- Durée d'emprunt : 14 jours, renouvelable une fois
- Silence requis dans les espaces de lecture
- Nourriture et boissons interdites

Services disponibles :
- Postes informatiques avec accès internet
- Imprimantes et photocopieurs
- Salles d'étude de groupe (réservation requise)
- Accès aux bases de données académiques
- Assistance des bibliothécaires

Contact :
- Email : bibliotheque@ihec.ucar.tn
- Téléphone : +216 71 775 948
- Bureau des bibliothécaires : 1er étage, salle 105
";

                var prompt = $@"Tu es HEC 1.0, l'assistant virtuel de la bibliothèque IHEC Carthage. 
Un utilisateur te demande des informations sur la bibliothèque avec la question suivante : '{query}'.
Utilise les informations ci-dessous pour répondre de manière précise et concise en français.

{libraryInfo}";

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
                                    text = prompt
                                }
                            }
                        }
                    }
                };

                request.AddJsonBody(JsonConvert.SerializeObject(payload));
                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    // Add null check for response content before deserializing
                    if (string.IsNullOrEmpty(response.Content))
                    {
                        Console.WriteLine("Error: Empty response content from Gemini API in GetLibraryInformationAsync.");
                        return "Je suis désolé, la réponse du service est vide. Veuillez réessayer.";
                    }

                    var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(response.Content);
                     // Add null check and use FirstOrDefault for safer access
                    if (geminiResponse == null)
                    {
                         Console.WriteLine("Error: Failed to deserialize Gemini response in GetLibraryInformationAsync.");
                         return "Je suis désolé, la réponse du service est invalide. Veuillez réessayer.";
                    }
                    return geminiResponse.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text
                           ?? "Je suis désolé, je n'ai pas pu traiter votre demande.";
                }
                else
                {
                    Console.WriteLine($"Error from Gemini API in GetLibraryInformationAsync: {response.StatusCode} - {response.ErrorMessage}");
                    return "Je suis désolé, je rencontre des difficultés techniques. Veuillez réessayer plus tard.";
                }
            }
            catch (JsonException jsonEx) // Catch specific JSON errors
            {
                Console.WriteLine($"JSON Deserialization Error in GetLibraryInformationAsync: {jsonEx.Message}");
                return "Je suis désolé, une erreur s'est produite lors du traitement de la réponse. Veuillez réessayer.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLibraryInformationAsync: {ex.Message}");
                return "Je suis désolé, une erreur s'est produite. Veuillez réessayer plus tard.";
            }
        }

        private List<string> ExtractKeywords(string query)
        {
            // Liste de mots à ignorer (articles, prépositions, etc.)
            var stopWords = new List<string> { "le", "la", "les", "un", "une", "des", "du", "de", "à", "au", "aux", "et", "ou", "je", "tu", "il", "elle", "nous", "vous", "ils", "elles", "ce", "cette", "ces", "mon", "ma", "mes", "ton", "ta", "tes", "son", "sa", "ses", "notre", "nos", "votre", "vos", "leur", "leurs", "que", "qui", "quoi", "comment", "pourquoi", "quand", "où", "livre", "livres", "recommande", "recommander", "suggère", "suggérer", "cherche", "chercher", "trouve", "trouver" };
            
            // Convertir en minuscules et diviser en mots
            var words = query.ToLower()
                .Replace("?", "")
                .Replace("!", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(";", "")
                .Replace(":", "")
                .Split(' ')
                .Where(w => w.Length > 2 && !stopWords.Contains(w))
                .Distinct()
                .ToList();
            
            return words;
        }

        private List<string> GetDefaultSuggestions()
        {
            return new List<string>
            {
                "Recommande-moi des livres",
                "Comment emprunter un livre ?",
                "Quels sont les horaires de la bibliothèque ?",
                "Aide-moi avec ma recherche"
            };
        }

        private List<string> GetSuggestionsForUserMessage(string userMessage)
        {
             if (userMessage.ToLower().Contains("livre") || userMessage.ToLower().Contains("recommand"))
             {
                 return new List<string>
                 {
                     "Montre-moi des livres de finance",
                     "Livres populaires en marketing",
                     "Comment emprunter un livre ?",
                     "Quels sont les livres disponibles en BI ?"
                 };
             }
             else if (userMessage.ToLower().Contains("horaire") || userMessage.ToLower().Contains("ouvert"))
             {
                 return new List<string>
                 {
                     "Comment réserver une salle d'étude ?",
                     "Quelles sont les règles de la bibliothèque ?",
                     "Y a-t-il des ordinateurs disponibles ?",
                     "Comment contacter un bibliothécaire ?"
                 };
             }
             else if (userMessage.ToLower().Contains("recherche") || userMessage.ToLower().Contains("étude"))
             {
                 return new List<string>
                 {
                     "Aide-moi avec ma recherche en finance",
                     "Comment citer des sources ?",
                     "Ressources pour un mémoire",
                     "Bases de données académiques disponibles"
                 };
             }
             else
             {
                 return GetDefaultSuggestions();
             }
        }
    }

    // Classes pour désérialiser la réponse de l'API Gemini
    public class GeminiResponse
    {
        public List<Candidate>? candidates { get; set; }
    }

    public class Candidate
    {
        public Content? content { get; set; }
    }

    public class Content
    {
        public List<Part>? parts { get; set; }
    }

    public class Part
    {
        public string? text { get; set; }
    }
}
