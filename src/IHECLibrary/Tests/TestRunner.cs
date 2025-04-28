using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IHECLibrary.Services;
using IHECLibrary.Services.Implementations;
using Serilog;

namespace IHECLibrary.Tests
{
    public class TestRunner
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IBookService _bookService;
        private readonly IChatbotService _chatbotService;
        private readonly IAdminService _adminService;
        private ILogger _logger = Serilog.Log.Logger; // Initialize with default logger
        private string _logFilePath;

        public TestRunner(
            IAuthService authService,
            IUserService userService,
            IBookService bookService,
            IChatbotService chatbotService,
            IAdminService adminService)
        {
            _authService = authService;
            _userService = userService;
            _bookService = bookService;
            _chatbotService = chatbotService;
            _adminService = adminService;
            
            // Configuration du logger par défaut
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_results.log");
            ConfigureLogger(_logFilePath);
        }
        
        public void SetLogPath(string logFilePath)
        {
            _logFilePath = logFilePath;
            ConfigureLogger(_logFilePath);
        }
        
        private void ConfigureLogger(string logFilePath)
        {
            _logger = new LoggerConfiguration()
                .WriteTo.File(logFilePath)
                .CreateLogger();
        }

        public async Task RunAllTests(bool isIteration = false, int currentIteration = 1, int totalIterations = 1)
        {
            if (isIteration)
            {
                _logger.Information("=== Itération {CurrentIteration}/{TotalIterations} des tests de l'application IHEC Library ===", 
                    currentIteration, totalIterations);
            }
            else
            {
                _logger.Information("=== Début des tests de l'application IHEC Library ===");
            }
            
            _logger.Information($"Date et heure: {DateTime.Now}");
            
            try
            {
                await TestAuthentication(isIteration);
                await TestUserService(isIteration);
                await TestBookService(isIteration);
                await TestChatbotService(isIteration);
                await TestAdminService(isIteration);
                
                if (isIteration)
                {
                    _logger.Information("=== Fin de l'itération {CurrentIteration}/{TotalIterations} ===", 
                        currentIteration, totalIterations);
                }
                else
                {
                    _logger.Information("=== Tous les tests ont été exécutés avec succès ===");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Une erreur s'est produite pendant les tests: {ex.Message}");
                _logger.Error("Stack trace: {StackTrace}", ex.StackTrace ?? "No stack trace available");
                
                if (isIteration)
                {
                    _logger.Error("=== Échec de l'itération {CurrentIteration}/{TotalIterations} ===",
                        currentIteration, totalIterations);
                }
            }
            
            // Ajouter une ligne séparatrice entre les itérations
            if (isIteration && currentIteration < totalIterations)
            {
                _logger.Information("------------------------------------------------");
            }
        }

        private async Task TestAuthentication(bool isIteration = false)
        {
            string testPrefix = isIteration ? $"[Iteration] " : "";
            _logger.Information("{Prefix}--- Test du service d'authentification ---", testPrefix);
            
            try
            {
                // Test d'inscription
                var registrationModel = new UserRegistrationModel
                {
                    Email = $"test_{Guid.NewGuid()}@test.com",
                    Password = "Test@123",
                    FirstName = "Test",
                    LastName = "User",
                    PhoneNumber = "+21612345678",
                    LevelOfStudy = "1",
                    FieldOfStudy = "Informatique"
                };
                
                var registerResult = await _authService.RegisterAsync(registrationModel);
                _logger.Information("{Prefix}Inscription: {Result}", testPrefix, 
                    (registerResult.Success ? "Réussie" : "Échouée"));
                if (!registerResult.Success)
                {
                    _logger.Error("{Prefix}Erreur d'inscription: {Error}", testPrefix, registerResult.ErrorMessage);
                }
                
                // Test de connexion
                var loginResult = await _authService.SignInAsync(registrationModel.Email, registrationModel.Password);
                _logger.Information("{Prefix}Connexion: {Result}", testPrefix, 
                    (loginResult.Success ? "Réussie" : "Échouée"));
                if (!loginResult.Success)
                {
                    _logger.Error("{Prefix}Erreur de connexion: {Error}", testPrefix, loginResult.ErrorMessage);
                }
                
                // Test de déconnexion
                var logoutResult = await _authService.SignOutAsync();
                _logger.Information("{Prefix}Déconnexion: {Result}", testPrefix, 
                    (logoutResult ? "Réussie" : "Échouée"));
                
                _logger.Information("{Prefix}Tests d'authentification terminés avec succès", testPrefix);
            }
            catch (Exception ex)
            {
                _logger.Error("{Prefix}Erreur lors des tests d'authentification: {Error}", testPrefix, ex.Message);
                throw;
            }
        }
        
        // Mettre à jour les autres méthodes de test de manière similaire pour prendre en charge le mode itératif
        private async Task TestUserService(bool isIteration = false)
        {
            string testPrefix = isIteration ? $"[Iteration] " : "";
            _logger.Information("{Prefix}--- Test du service utilisateur ---", testPrefix);
            
            try
            {
                // Se connecter d'abord
                var loginResult = await _authService.SignInAsync("test@ihec.ucar.tn", "Test@123");
                if (!loginResult.Success)
                {
                    _logger.Warning("{Prefix}Impossible de se connecter pour tester le service utilisateur", testPrefix);
                    return;
                }
                
                // Test de récupération de l'utilisateur actuel
                var currentUser = await _userService.GetCurrentUserAsync();
                _logger.Information("{Prefix}Récupération de l'utilisateur actuel: {Result}", testPrefix, 
                    (currentUser != null ? "Réussie" : "Échouée"));
                if (currentUser != null)
                {
                    _logger.Information("{Prefix}Utilisateur: {FirstName} {LastName}", testPrefix, 
                        currentUser.FirstName, currentUser.LastName);
                }
                
                // Test de mise à jour du profil
                if (currentUser != null)
                {
                    var updateModel = new UserProfileUpdateModel
                    {
                        FirstName = currentUser.FirstName,
                        LastName = currentUser.LastName,
                        PhoneNumber = currentUser.PhoneNumber,
                        LevelOfStudy = currentUser.LevelOfStudy,
                        FieldOfStudy = currentUser.FieldOfStudy
                    };
                    
                    var updateResult = await _userService.UpdateUserProfileAsync(updateModel);
                    _logger.Information("{Prefix}Mise à jour du profil: {Result}", testPrefix, 
                        (updateResult ? "Réussie" : "Échouée"));
                }
                
                // Test de récupération des statistiques
                if (currentUser != null)
                {
                    var statistics = await _userService.GetUserStatisticsAsync(currentUser.Id);
                    _logger.Information("{Prefix}Récupération des statistiques: {Result}", testPrefix, 
                        (statistics != null ? "Réussie" : "Échouée"));
                    if (statistics != null)
                    {
                        _logger.Information("{Prefix}Rang: {Ranking}", testPrefix, statistics.Ranking);
                        _logger.Information("{Prefix}Livres empruntés: {Count}", testPrefix, statistics.BorrowedBooksCount);
                        _logger.Information("{Prefix}Livres réservés: {Count}", testPrefix, statistics.ReservedBooksCount);
                        _logger.Information("{Prefix}Livres aimés: {Count}", testPrefix, statistics.LikedBooksCount);
                    }
                }
                
                _logger.Information("{Prefix}Tests du service utilisateur terminés avec succès", testPrefix);
            }
            catch (Exception ex)
            {
                _logger.Error("{Prefix}Erreur lors des tests du service utilisateur: {Error}", testPrefix, ex.Message);
                throw;
            }
            finally
            {
                // Se déconnecter
                await _authService.SignOutAsync();
            }
        }

        private async Task TestBookService(bool isIteration = false)
        {
            string testPrefix = isIteration ? $"[Iteration] " : "";
            _logger.Information("{Prefix}--- Test du service de livres ---", testPrefix);
            
            try
            {
                // Se connecter d'abord
                var loginResult = await _authService.SignInAsync("test@ihec.ucar.tn", "Test@123");
                if (!loginResult.Success)
                {
                    _logger.Warning("{Prefix}Impossible de se connecter pour tester le service de livres", testPrefix);
                    return;
                }
                
                // Test de récupération des livres recommandés
                var recommendedBooks = await _bookService.GetRecommendedBooksAsync();
                _logger.Information("{Prefix}Récupération des livres recommandés: {Result}", testPrefix, 
                    (recommendedBooks.Count > 0 ? "Réussie" : "Aucun livre trouvé"));
                _logger.Information("{Prefix}Nombre de livres recommandés: {Count}", testPrefix, recommendedBooks.Count);
                
                // Test de recherche de livres
                var searchResults = await _bookService.GetBooksBySearchAsync("data");
                _logger.Information("{Prefix}Recherche de livres: {Result}", testPrefix, 
                    (searchResults.Count > 0 ? "Réussie" : "Aucun livre trouvé"));
                _logger.Information("{Prefix}Nombre de résultats: {Count}", testPrefix, searchResults.Count);
                
                // Test de récupération des livres par catégorie
                var categoryBooks = await _bookService.GetBooksByCategoryAsync("Informatique");
                _logger.Information("{Prefix}Récupération des livres par catégorie: {Result}", testPrefix, 
                    (categoryBooks.Count > 0 ? "Réussie" : "Aucun livre trouvé"));
                _logger.Information("{Prefix}Nombre de livres dans la catégorie: {Count}", testPrefix, categoryBooks.Count);
                
                // Test de récupération d'un livre par ID
                if (searchResults.Count > 0)
                {
                    var bookId = searchResults[0].Id;
                    var book = await _bookService.GetBookByIdAsync(bookId);
                    _logger.Information("{Prefix}Récupération d'un livre par ID: {Result}", testPrefix, 
                        (book != null ? "Réussie" : "Échouée"));
                    if (book != null)
                    {
                        _logger.Information("{Prefix}Livre: {Title} par {Author}", testPrefix, book.Title, book.Author);
                        
                        // Test d'emprunt de livre
                        var borrowResult = await _bookService.BorrowBookAsync(bookId);
                        _logger.Information("{Prefix}Emprunt de livre: {Result}", testPrefix, 
                            (borrowResult ? "Réussi" : "Échoué"));
                        
                        // Test de retour de livre
                        var returnResult = await _bookService.ReturnBookAsync(bookId);
                        _logger.Information("{Prefix}Retour de livre: {Result}", testPrefix, 
                            (returnResult ? "Réussi" : "Échoué"));
                        
                        // Test de réservation de livre
                        var reserveResult = await _bookService.ReserveBookAsync(bookId);
                        _logger.Information("{Prefix}Réservation de livre: {Result}", testPrefix, 
                            (reserveResult ? "Réussie" : "Échouée"));
                        
                        // Test d'évaluation de livre
                        var rateResult = await _bookService.RateBookAsync(bookId, 5, "Excellent livre !");
                        _logger.Information("{Prefix}Évaluation de livre: {Result}", testPrefix, 
                            (rateResult ? "Réussie" : "Échouée"));
                        
                        // Test de like/unlike de livre
                        var likeResult = await _bookService.LikeBookAsync(bookId);
                        _logger.Information("{Prefix}Like de livre: {Result}", testPrefix, 
                            (likeResult ? "Réussi" : "Échoué"));
                        
                        var unlikeResult = await _bookService.UnlikeBookAsync(bookId);
                        _logger.Information("{Prefix}Unlike de livre: {Result}", testPrefix, 
                            (unlikeResult ? "Réussi" : "Échoué"));
                    }
                }
                
                _logger.Information("{Prefix}Tests du service de livres terminés avec succès", testPrefix);
            }
            catch (Exception ex)
            {
                _logger.Error("{Prefix}Erreur lors des tests du service de livres: {Error}", testPrefix, ex.Message);
                throw;
            }
            finally
            {
                // Se déconnecter
                await _authService.SignOutAsync();
            }
        }

        public async Task<bool> TestBookSearch()
        {
            try
            {
                _logger.Information("Exécution du test de recherche de livres...");
                
                // Effectuer une recherche de livres
                var books = await _bookService.GetBooksBySearchAsync("programmation");
                
                // Vérifier si la recherche a retourné des résultats
                if (books != null && books.Any())
                {
                    _logger.Information($"La recherche a trouvé {books.Count} livres.");
                    
                    // Vérifier les détails d'un livre
                    var firstBook = books.FirstOrDefault();
                    if (firstBook != null)
                    {
                        _logger.Information($"Premier livre trouvé: {firstBook.Title} par {firstBook.Author}");
                        return true;
                    }
                }
                
                _logger.Error("La recherche n'a pas trouvé de livres.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors du test de recherche de livres: {ex.Message}");
                return false;
            }
        }

        private async Task TestChatbotService(bool isIteration = false)
        {
            string testPrefix = isIteration ? $"[Iteration] " : "";
            _logger.Information("{Prefix}--- Test du service de chatbot ---", testPrefix);
            
            try
            {
                // Test de réponse du chatbot
                var response = await _chatbotService.GetResponseAsync("Bonjour, pouvez-vous me recommander des livres sur la finance ?");
                _logger.Information("{Prefix}Réponse du chatbot: {Result}", testPrefix, 
                    (response != null ? "Réussie" : "Échouée"));
                if (response != null)
                {
                    _logger.Information("{Prefix}Message: {Message}...", testPrefix, 
                        response.Message.Substring(0, Math.Min(100, response.Message.Length)));
                    _logger.Information("{Prefix}Nombre de suggestions: {Count}", testPrefix, 
                        response.Suggestions?.Count ?? 0);
                    _logger.Information("{Prefix}Nombre de recommandations de livres: {Count}", testPrefix, 
                        response.BookRecommendations?.Count ?? 0);
                }
                
                // Test d'assistance à la recherche
                var researchAssistance = await _chatbotService.GetResearchAssistanceAsync("Intelligence artificielle dans la finance");
                _logger.Information("{Prefix}Assistance à la recherche: {Result}", testPrefix, 
                    (!string.IsNullOrEmpty(researchAssistance) ? "Réussie" : "Échouée"));
                if (!string.IsNullOrEmpty(researchAssistance))
                {
                    _logger.Information("{Prefix}Réponse: {Response}...", testPrefix, 
                        researchAssistance.Substring(0, Math.Min(100, researchAssistance.Length)));
                }
                
                // Test d'informations sur la bibliothèque
                var libraryInfo = await _chatbotService.GetLibraryInformationAsync("Quels sont les horaires d'ouverture ?");
                _logger.Information("{Prefix}Informations sur la bibliothèque: {Result}", testPrefix, 
                    (!string.IsNullOrEmpty(libraryInfo) ? "Réussie" : "Échouée"));
                if (!string.IsNullOrEmpty(libraryInfo))
                {
                    _logger.Information("{Prefix}Réponse: {Response}...", testPrefix, 
                        libraryInfo.Substring(0, Math.Min(100, libraryInfo.Length)));
                }
                
                _logger.Information("{Prefix}Tests du service de chatbot terminés avec succès", testPrefix);
            }
            catch (Exception ex)
            {
                _logger.Error("{Prefix}Erreur lors des tests du service de chatbot: {Error}", testPrefix, ex.Message);
                throw;
            }
        }

        private async Task TestAdminService(bool isIteration = false)
        {
            string testPrefix = isIteration ? $"[Iteration] " : "";
            _logger.Information("{Prefix}--- Test du service administrateur ---", testPrefix);
            
            try
            {
                // Se connecter en tant qu'administrateur
                var loginResult = await _authService.SignInAsync("admin@ihec.ucar.tn", "Admin@123");
                if (!loginResult.Success)
                {
                    _logger.Warning("{Prefix}Impossible de se connecter en tant qu'administrateur pour tester le service d'administration", testPrefix);
                    return;
                }
                
                // Test de récupération de l'administrateur actuel
                var currentAdmin = await _adminService.GetCurrentAdminAsync();
                _logger.Information("{Prefix}Récupération de l'administrateur actuel: {Result}", testPrefix, 
                    (currentAdmin != null ? "Réussie" : "Échouée"));
                if (currentAdmin != null)
                {
                    _logger.Information("{Prefix}Administrateur: {FirstName} {LastName}", testPrefix, 
                        currentAdmin.FirstName, currentAdmin.LastName);
                }
                
                // Test de récupération des données du tableau de bord
                var dashboardData = await _adminService.GetDashboardDataAsync();
                _logger.Information("{Prefix}Récupération des données du tableau de bord: {Result}", testPrefix, 
                    (dashboardData != null ? "Réussie" : "Échouée"));
                if (dashboardData != null)
                {
                    _logger.Information("{Prefix}Nombre total de livres: {Count}", testPrefix, dashboardData.TotalBooksCount);
                    _logger.Information("{Prefix}Nombre total d'utilisateurs: {Count}", testPrefix, dashboardData.TotalUsersCount);
                    _logger.Information("{Prefix}Nombre d'emprunts actifs: {Count}", testPrefix, dashboardData.ActiveBorrowingsCount);
                    _logger.Information("{Prefix}Nombre de réservations en attente: {Count}", testPrefix, dashboardData.PendingReservationsCount);
                }
                
                // Test de récupération de tous les livres
                var allBooks = await _adminService.GetAllBooksAsync();
                _logger.Information("{Prefix}Récupération de tous les livres: {Result}", testPrefix, 
                    (allBooks.Count > 0 ? "Réussie" : "Aucun livre trouvé"));
                _logger.Information("{Prefix}Nombre de livres: {Count}", testPrefix, allBooks.Count);
                
                // Test de récupération de tous les utilisateurs
                var allUsers = await _adminService.GetAllUsersAsync();
                _logger.Information("{Prefix}Récupération de tous les utilisateurs: {Result}", testPrefix, 
                    (allUsers.Count > 0 ? "Réussie" : "Aucun utilisateur trouvé"));
                _logger.Information("{Prefix}Nombre d'utilisateurs: {Count}", testPrefix, allUsers.Count);
                
                // Test de récupération de tous les emprunts
                var allBorrowings = await _adminService.GetAllBorrowingsAsync();
                _logger.Information("{Prefix}Récupération de tous les emprunts: {Result}", testPrefix, 
                    (allBorrowings.Count > 0 ? "Réussie" : "Aucun emprunt trouvé"));
                _logger.Information("{Prefix}Nombre d'emprunts: {Count}", testPrefix, allBorrowings.Count);
                
                // Test de récupération de toutes les réservations
                var allReservations = await _adminService.GetAllReservationsAsync();
                _logger.Information("{Prefix}Récupération de toutes les réservations: {Result}", testPrefix, 
                    (allReservations.Count > 0 ? "Réussie" : "Aucune réservation trouvée"));
                _logger.Information("{Prefix}Nombre de réservations: {Count}", testPrefix, allReservations.Count);
                
                // Test d'ajout de livre
                var bookAddModel = new BookAddModel
                {
                    Title = "Livre de test",
                    Author = "Auteur de test",
                    ISBN = "1234567890123",
                    PublicationYear = 2025,
                    Publisher = "Éditeur de test",
                    Category = "Test",
                    Description = "Description du livre de test",
                    TotalCopies = 5
                };
                
                var addBookResult = await _adminService.AddBookAsync(bookAddModel);
                _logger.Information("{Prefix}Ajout de livre: {Result}", testPrefix, 
                    (addBookResult ? "Réussi" : "Échoué"));
                
                // Trouver le livre ajouté pour les tests suivants
                var addedBooks = await _adminService.GetAllBooksAsync();
                var addedBook = addedBooks.FirstOrDefault(b => b.Title == "Livre de test");
                
                if (addedBook != null)
                {
                    // Test de mise à jour de livre
                    var bookUpdateModel = new BookUpdateModel
                    {
                        Id = addedBook.Id,
                        Title = "Livre de test mis à jour",
                        Author = "Auteur de test",
                        ISBN = "1234567890123",
                        PublicationYear = 2025,
                        Publisher = "Éditeur de test",
                        Category = "Test",
                        Description = "Description mise à jour du livre de test",
                        TotalCopies = 10
                    };
                    
                    var updateBookResult = await _adminService.UpdateBookAsync(bookUpdateModel);
                    _logger.Information("{Prefix}Mise à jour de livre: {Result}", testPrefix, 
                        (updateBookResult ? "Réussie" : "Échouée"));
                    
                    // Test de suppression de livre
                    var deleteBookResult = await _adminService.DeleteBookAsync(addedBook.Id);
                    _logger.Information("{Prefix}Suppression de livre: {Result}", testPrefix, 
                        (deleteBookResult ? "Réussie" : "Échouée"));
                }
                
                _logger.Information("{Prefix}Tests du service administrateur terminés avec succès", testPrefix);
            }
            catch (Exception ex)
            {
                _logger.Error("{Prefix}Erreur lors des tests du service administrateur: {Error}", testPrefix, ex.Message);
                throw;
            }
            finally
            {
                // Se déconnecter
                await _authService.SignOutAsync();
            }
        }
    }
}
