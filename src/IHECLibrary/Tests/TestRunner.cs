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
        private readonly ILogger _logger;
        private readonly string _logFilePath;

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
            
            // Configuration du logger
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_results.log");
            _logger = new LoggerConfiguration()
                .WriteTo.File(_logFilePath)
                .CreateLogger();
        }

        public async Task RunAllTests()
        {
            _logger.Information("=== Début des tests de l'application IHEC Library ===");
            _logger.Information($"Date et heure: {DateTime.Now}");
            
            try
            {
                await TestAuthentication();
                await TestUserService();
                await TestBookService();
                await TestChatbotService();
                await TestAdminService();
                
                _logger.Information("=== Tous les tests ont été exécutés avec succès ===");
            }
            catch (Exception ex)
            {
                _logger.Error($"Une erreur s'est produite pendant les tests: {ex.Message}");
                _logger.Error("Stack trace: {StackTrace}", ex.StackTrace ?? "No stack trace available");
            }
        }

        private async Task TestAuthentication()
        {
            _logger.Information("--- Test du service d'authentification ---");
            
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
                _logger.Information($"Inscription: {(registerResult.Success ? "Réussie" : "Échouée")}");
                if (!registerResult.Success)
                {
                    _logger.Error($"Erreur d'inscription: {registerResult.ErrorMessage}");
                }
                
                // Test de connexion
                var loginResult = await _authService.SignInAsync(registrationModel.Email, registrationModel.Password);
                _logger.Information($"Connexion: {(loginResult.Success ? "Réussie" : "Échouée")}");
                if (!loginResult.Success)
                {
                    _logger.Error($"Erreur de connexion: {loginResult.ErrorMessage}");
                }
                
                // Test de déconnexion
                var logoutResult = await _authService.SignOutAsync();
                _logger.Information($"Déconnexion: {(logoutResult ? "Réussie" : "Échouée")}");
                
                _logger.Information("Tests d'authentification terminés avec succès");
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors des tests d'authentification: {ex.Message}");
                throw;
            }
        }

        private async Task TestUserService()
        {
            _logger.Information("--- Test du service utilisateur ---");
            
            try
            {
                // Se connecter d'abord
                var loginResult = await _authService.SignInAsync("test@ihec.ucar.tn", "Test@123");
                if (!loginResult.Success)
                {
                    _logger.Warning("Impossible de se connecter pour tester le service utilisateur");
                    return;
                }
                
                // Test de récupération de l'utilisateur actuel
                var currentUser = await _userService.GetCurrentUserAsync();
                _logger.Information($"Récupération de l'utilisateur actuel: {(currentUser != null ? "Réussie" : "Échouée")}");
                if (currentUser != null)
                {
                    _logger.Information($"Utilisateur: {currentUser.FirstName} {currentUser.LastName}");
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
                    _logger.Information($"Mise à jour du profil: {(updateResult ? "Réussie" : "Échouée")}");
                }
                
                // Test de récupération des statistiques
                if (currentUser != null)
                {
                    var statistics = await _userService.GetUserStatisticsAsync(currentUser.Id);
                    _logger.Information($"Récupération des statistiques: {(statistics != null ? "Réussie" : "Échouée")}");
                    if (statistics != null)
                    {
                        _logger.Information($"Rang: {statistics.Ranking}");
                        _logger.Information($"Livres empruntés: {statistics.BorrowedBooksCount}");
                        _logger.Information($"Livres réservés: {statistics.ReservedBooksCount}");
                        _logger.Information($"Livres aimés: {statistics.LikedBooksCount}");
                    }
                }
                
                _logger.Information("Tests du service utilisateur terminés avec succès");
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors des tests du service utilisateur: {ex.Message}");
                throw;
            }
            finally
            {
                // Se déconnecter
                await _authService.SignOutAsync();
            }
        }

        private async Task TestBookService()
        {
            _logger.Information("--- Test du service de gestion des livres ---");
            
            try
            {
                // Se connecter d'abord
                var loginResult = await _authService.SignInAsync("test@ihec.ucar.tn", "Test@123");
                if (!loginResult.Success)
                {
                    _logger.Warning("Impossible de se connecter pour tester le service de livres");
                    return;
                }
                
                // Test de récupération des livres recommandés
                var recommendedBooks = await _bookService.GetRecommendedBooksAsync();
                _logger.Information($"Récupération des livres recommandés: {(recommendedBooks.Count > 0 ? "Réussie" : "Aucun livre trouvé")}");
                _logger.Information($"Nombre de livres recommandés: {recommendedBooks.Count}");
                
                // Test de recherche de livres
                var searchResults = await _bookService.GetBooksBySearchAsync("data");
                _logger.Information($"Recherche de livres: {(searchResults.Count > 0 ? "Réussie" : "Aucun livre trouvé")}");
                _logger.Information($"Nombre de résultats: {searchResults.Count}");
                
                // Test de récupération des livres par catégorie
                var categoryBooks = await _bookService.GetBooksByCategoryAsync("Informatique");
                _logger.Information($"Récupération des livres par catégorie: {(categoryBooks.Count > 0 ? "Réussie" : "Aucun livre trouvé")}");
                _logger.Information($"Nombre de livres dans la catégorie: {categoryBooks.Count}");
                
                // Test de récupération d'un livre par ID
                if (searchResults.Count > 0)
                {
                    var bookId = searchResults[0].Id;
                    var book = await _bookService.GetBookByIdAsync(bookId);
                    _logger.Information($"Récupération d'un livre par ID: {(book != null ? "Réussie" : "Échouée")}");
                    if (book != null)
                    {
                        _logger.Information($"Livre: {book.Title} par {book.Author}");
                        
                        // Test d'emprunt de livre
                        var borrowResult = await _bookService.BorrowBookAsync(bookId);
                        _logger.Information($"Emprunt de livre: {(borrowResult ? "Réussi" : "Échoué")}");
                        
                        // Test de retour de livre
                        var returnResult = await _bookService.ReturnBookAsync(bookId);
                        _logger.Information($"Retour de livre: {(returnResult ? "Réussi" : "Échoué")}");
                        
                        // Test de réservation de livre
                        var reserveResult = await _bookService.ReserveBookAsync(bookId);
                        _logger.Information($"Réservation de livre: {(reserveResult ? "Réussie" : "Échouée")}");
                        
                        // Test d'évaluation de livre
                        var rateResult = await _bookService.RateBookAsync(bookId, 5, "Excellent livre !");
                        _logger.Information($"Évaluation de livre: {(rateResult ? "Réussie" : "Échouée")}");
                        
                        // Test de like/unlike de livre
                        var likeResult = await _bookService.LikeBookAsync(bookId);
                        _logger.Information($"Like de livre: {(likeResult ? "Réussi" : "Échoué")}");
                        
                        var unlikeResult = await _bookService.UnlikeBookAsync(bookId);
                        _logger.Information($"Unlike de livre: {(unlikeResult ? "Réussi" : "Échoué")}");
                    }
                }
                
                _logger.Information("Tests du service de gestion des livres terminés avec succès");
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors des tests du service de gestion des livres: {ex.Message}");
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

        private async Task TestChatbotService()
        {
            _logger.Information("--- Test du service de chatbot ---");
            
            try
            {
                // Test de réponse du chatbot
                var response = await _chatbotService.GetResponseAsync("Bonjour, pouvez-vous me recommander des livres sur la finance ?");
                _logger.Information($"Réponse du chatbot: {(response != null ? "Réussie" : "Échouée")}");
                if (response != null)
                {
                    _logger.Information($"Message: {response.Message.Substring(0, Math.Min(100, response.Message.Length))}...");
                    _logger.Information($"Nombre de suggestions: {response.Suggestions?.Count ?? 0}");
                    _logger.Information($"Nombre de recommandations de livres: {response.BookRecommendations?.Count ?? 0}");
                }
                
                // Test d'assistance à la recherche
                var researchAssistance = await _chatbotService.GetResearchAssistanceAsync("Intelligence artificielle dans la finance");
                _logger.Information($"Assistance à la recherche: {(!string.IsNullOrEmpty(researchAssistance) ? "Réussie" : "Échouée")}");
                if (!string.IsNullOrEmpty(researchAssistance))
                {
                    _logger.Information($"Réponse: {researchAssistance.Substring(0, Math.Min(100, researchAssistance.Length))}...");
                }
                
                // Test d'informations sur la bibliothèque
                var libraryInfo = await _chatbotService.GetLibraryInformationAsync("Quels sont les horaires d'ouverture ?");
                _logger.Information($"Informations sur la bibliothèque: {(!string.IsNullOrEmpty(libraryInfo) ? "Réussie" : "Échouée")}");
                if (!string.IsNullOrEmpty(libraryInfo))
                {
                    _logger.Information($"Réponse: {libraryInfo.Substring(0, Math.Min(100, libraryInfo.Length))}...");
                }
                
                _logger.Information("Tests du service de chatbot terminés avec succès");
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors des tests du service de chatbot: {ex.Message}");
                throw;
            }
        }

        private async Task TestAdminService()
        {
            _logger.Information("--- Test du service d'administration ---");
            
            try
            {
                // Se connecter en tant qu'administrateur
                var loginResult = await _authService.SignInAsync("admin@ihec.ucar.tn", "Admin@123");
                if (!loginResult.Success)
                {
                    _logger.Warning("Impossible de se connecter en tant qu'administrateur pour tester le service d'administration");
                    return;
                }
                
                // Test de récupération de l'administrateur actuel
                var currentAdmin = await _adminService.GetCurrentAdminAsync();
                _logger.Information($"Récupération de l'administrateur actuel: {(currentAdmin != null ? "Réussie" : "Échouée")}");
                if (currentAdmin != null)
                {
                    _logger.Information($"Administrateur: {currentAdmin.FirstName} {currentAdmin.LastName}");
                }
                
                // Test de récupération des données du tableau de bord
                var dashboardData = await _adminService.GetDashboardDataAsync();
                _logger.Information($"Récupération des données du tableau de bord: {(dashboardData != null ? "Réussie" : "Échouée")}");
                if (dashboardData != null)
                {
                    _logger.Information($"Nombre total de livres: {dashboardData.TotalBooksCount}");
                    _logger.Information($"Nombre total d'utilisateurs: {dashboardData.TotalUsersCount}");
                    _logger.Information($"Nombre d'emprunts actifs: {dashboardData.ActiveBorrowingsCount}");
                    _logger.Information($"Nombre de réservations en attente: {dashboardData.PendingReservationsCount}");
                }
                
                // Test de récupération de tous les livres
                var allBooks = await _adminService.GetAllBooksAsync();
                _logger.Information($"Récupération de tous les livres: {(allBooks.Count > 0 ? "Réussie" : "Aucun livre trouvé")}");
                _logger.Information($"Nombre de livres: {allBooks.Count}");
                
                // Test de récupération de tous les utilisateurs
                var allUsers = await _adminService.GetAllUsersAsync();
                _logger.Information($"Récupération de tous les utilisateurs: {(allUsers.Count > 0 ? "Réussie" : "Aucun utilisateur trouvé")}");
                _logger.Information($"Nombre d'utilisateurs: {allUsers.Count}");
                
                // Test de récupération de tous les emprunts
                var allBorrowings = await _adminService.GetAllBorrowingsAsync();
                _logger.Information($"Récupération de tous les emprunts: {(allBorrowings.Count > 0 ? "Réussie" : "Aucun emprunt trouvé")}");
                _logger.Information($"Nombre d'emprunts: {allBorrowings.Count}");
                
                // Test de récupération de toutes les réservations
                var allReservations = await _adminService.GetAllReservationsAsync();
                _logger.Information($"Récupération de toutes les réservations: {(allReservations.Count > 0 ? "Réussie" : "Aucune réservation trouvée")}");
                _logger.Information($"Nombre de réservations: {allReservations.Count}");
                
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
                _logger.Information($"Ajout de livre: {(addBookResult ? "Réussi" : "Échoué")}");
                
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
                    _logger.Information($"Mise à jour de livre: {(updateBookResult ? "Réussie" : "Échouée")}");
                    
                    // Test de suppression de livre
                    var deleteBookResult = await _adminService.DeleteBookAsync(addedBook.Id);
                    _logger.Information($"Suppression de livre: {(deleteBookResult ? "Réussie" : "Échouée")}");
                }
                
                _logger.Information("Tests du service d'administration terminés avec succès");
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors des tests du service d'administration: {ex.Message}");
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
