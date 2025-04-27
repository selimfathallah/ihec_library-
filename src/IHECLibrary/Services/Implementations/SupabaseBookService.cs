using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Postgrest.Attributes;
using Postgrest.Models;
using Postgrest;
using Postgrest.Responses;
using IHECLibrary.Services.Models;  // This ensures the DbBook class from Models is used

namespace IHECLibrary.Services.Implementations
{
    public class SupabaseBookService : IBookService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly IUserService _userService;

        public SupabaseBookService(Supabase.Client supabaseClient, IUserService userService)
        {
            _supabaseClient = supabaseClient;
            _userService = userService;
        }

        public async Task<List<BookModel>> GetRecommendedBooksAsync()
        {
            try
            {
                Console.WriteLine("Début de la récupération des livres recommandés...");

                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    Console.WriteLine("Utilisateur actuel non trouvé");
                    return await GetFallbackBooksAsync(); // Utiliser la méthode de secours
                }

                Console.WriteLine($"Utilisateur trouvé: {currentUser.FirstName} {currentUser.LastName}");
                Console.WriteLine($"Domaine d'études: {currentUser.FieldOfStudy ?? "Non défini"}");

                // Obtenir tous les livres d'abord
                var books = await _supabaseClient.From<DbBook>().Get();
                Console.WriteLine($"Nombre total de livres récupérés: {books.Models.Count}");
                
                if (books.Models.Count == 0)
                {
                    return await GetFallbackBooksAsync(); // Utiliser la méthode de secours si aucun livre trouvé
                }

                // Afficher les 3 premiers livres pour le débogage
                for (int i = 0; i < Math.Min(3, books.Models.Count); i++)
                {
                    var book = books.Models[i];
                    Console.WriteLine($"  Livre {i+1}: {book.Title} / {book.Author} / {book.Category} / Status: {book.AvailabilityStatus}");
                }

                // Obtenir des recommandations basées sur le domaine d'études de l'utilisateur
                List<DbBook> selectedBooks = new List<DbBook>();
                
                if (!string.IsNullOrEmpty(currentUser.FieldOfStudy))
                {
                    Console.WriteLine($"Recherche de livres dans la catégorie: {currentUser.FieldOfStudy}");
                    var fieldBooks = books.Models
                        .Where(b => b.Category != null && 
                               b.Category.Equals(currentUser.FieldOfStudy, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                        
                    Console.WriteLine($"Nombre de livres dans la catégorie {currentUser.FieldOfStudy}: {fieldBooks.Count}");
                    selectedBooks.AddRange(fieldBooks);
                }

                // Si pas assez de livres trouvés par catégorie, ajouter des livres aléatoires
                if (selectedBooks.Count < 4)
                {
                    Console.WriteLine($"Pas assez de livres dans la catégorie, ajout de livres supplémentaires");
                    var remainingCount = 4 - selectedBooks.Count;
                    var additionalBooks = books.Models
                        .Where(b => !selectedBooks.Contains(b))
                        .Take(remainingCount)
                        .ToList();
                        
                    Console.WriteLine($"Ajout de {additionalBooks.Count} livres supplémentaires");
                    selectedBooks.AddRange(additionalBooks);
                }

                var bookModels = ConvertToBookModels(selectedBooks);
                Console.WriteLine($"Nombre final de livres recommandés: {bookModels.Count}");
                
                return bookModels;
            }
            catch (Exception ex)
            {
                // Journaliser l'erreur
                Console.WriteLine($"Erreur dans GetRecommendedBooksAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // Utiliser la méthode de secours pour éviter un échec complet
                return await GetFallbackBooksAsync();
            }
        }

        // Méthode de secours pour garantir l'affichage de livres même en cas d'erreur
        private async Task<List<BookModel>> GetFallbackBooksAsync()
        {
            try
            {
                Console.WriteLine("Utilisation de la méthode de secours pour obtenir des livres");
                
                // Simplement récupérer les 4 premiers livres sans aucun filtrage
                var books = await _supabaseClient.From<DbBook>().Get();
                var selectedBooks = books.Models.Take(4).ToList();
                
                Console.WriteLine($"Méthode de secours: {selectedBooks.Count} livres trouvés");
                
                return ConvertToBookModels(selectedBooks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur même dans la méthode de secours: {ex.Message}");
                return new List<BookModel>();
            }
        }

        public async Task<List<BookModel>> GetBooksBySearchAsync(string searchQuery)
        {
            try
            {
                var booksResult = await _supabaseClient.From<DbBook>().Get();
                
                // Filter in memory to avoid ambiguity errors
                var filteredBooks = booksResult.Models.Where(b => 
                    (b.Title != null && b.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (b.Author != null && b.Author.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (b.Description != null && b.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                
                return ConvertToBookModels(filteredBooks);
            }
            catch
            {
                return new List<BookModel>();
            }
        }

        public async Task<List<BookModel>> GetBooksByCategoryAsync(string category)
        {
            try
            {
                var booksResult = await _supabaseClient.From<DbBook>().Get();
                
                // Filter in memory to avoid ambiguity errors
                var filteredBooks = booksResult.Models.Where(b => 
                    b.Category != null && b.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
                ).ToList();
                
                return ConvertToBookModels(filteredBooks);
            }
            catch
            {
                return new List<BookModel>();
            }
        }

        public async Task<List<BookModel>> GetBooksByFiltersAsync(List<string> categories, bool availableOnly, string? language)
        {
            try
            {
                // Get all books and filter in memory
                var booksResult = await _supabaseClient.From<DbBook>().Get();
                var filteredBooks = booksResult.Models;
                
                // Apply category filter if specified
                if (categories != null && categories.Count > 0)
                {
                    filteredBooks = filteredBooks.Where(b => 
                        b.Category != null && categories.Contains(b.Category)
                    ).ToList();
                }
                
                // Apply availability filter if requested
                if (availableOnly)
                {
                    filteredBooks = filteredBooks.Where(b => 
                        b.AvailabilityStatus != null && b.AvailabilityStatus.Equals("Available", StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }
                
                return ConvertToBookModels(filteredBooks);
            }
            catch
            {
                return new List<BookModel>();
            }
        }

        public async Task<BookModel?> GetBookByIdAsync(string id)
        {
            try
            {
                var books = await _supabaseClient.From<DbBook>().Get();
                var book = books.Models.FirstOrDefault(b => b.BookId == id);

                if (book == null)
                    return null;

                // Récupérer les statistiques du livre
                var bookStatsResult = await _supabaseClient.From<DbBookStatistics>().Get();
                var bookStats = bookStatsResult.Models.FirstOrDefault(s => s.BookId == id);

                var bookModel = new BookModel
                {
                    Id = book.BookId ?? "",
                    Title = book.Title ?? "",
                    Author = book.Author ?? "",
                    ISBN = book.ISBN ?? "",
                    PublicationYear = book.PublicationYear,
                    Publisher = book.Publisher ?? "",
                    Category = book.Category ?? "",
                    Description = book.Description ?? "",
                    CoverImageUrl = !string.IsNullOrEmpty(book.CoverImageUrl) 
                        ? book.CoverImageUrl 
                        : $"https://via.placeholder.com/150?text={Uri.EscapeDataString(book.Title ?? "Book")}",
                    // Correction: Vérification insensible à la casse pour le statut
                    AvailableCopies = book.AvailabilityStatus?.Equals("Available", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0,
                    TotalCopies = 1 // Default value
                };

                // Ajouter les statistiques si disponibles
                if (bookStats != null)
                {
                    bookModel.LikesCount = bookStats.TotalLikes;
                }

                // Vérifier si l'utilisateur actuel a aimé ce livre
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser != null)
                {
                    var likesResult = await _supabaseClient.From<DbBookLike>().Get();
                    var like = likesResult.Models.FirstOrDefault(
                        l => l.BookId == id && l.UserId == currentUser.Id
                    );

                    bookModel.IsLikedByCurrentUser = like != null;
                }

                // Récupérer les évaluations du livre
                var ratingsResult = await _supabaseClient.From<DbBookRating>().Get();
                var ratings = ratingsResult.Models.Where(r => r.BookId == id).ToList();

                // Récupérer les commentaires du livre
                var commentsResult = await _supabaseClient.From<DbBookComment>().Get();
                var comments = commentsResult.Models.Where(c => c.BookId == id).ToList();

                // Créer un dictionnaire pour associer les commentaires aux utilisateurs
                var commentDict = new Dictionary<string, string>();
                foreach (var comment in comments)
                {
                    if (comment.UserId != null && comment.BookId != null && comment.CommentText != null)
                    {
                        commentDict[comment.UserId + "_" + comment.BookId] = comment.CommentText;
                    }
                }

                foreach (var rating in ratings)
                {
                    if (rating.UserId == null)
                        continue;
                        
                    var user = await _userService.GetUserByIdAsync(rating.UserId);
                    
                    // Essayer de trouver un commentaire associé
                    string comment = "";
                    if (rating.BookId != null && rating.UserId != null)
                    {
                        string commentKey = rating.UserId + "_" + rating.BookId;
                        if (commentDict.ContainsKey(commentKey))
                        {
                            comment = commentDict[commentKey];
                        }
                    }

                    bookModel.Ratings.Add(new BookRatingModel
                    {
                        Id = rating.Id.ToString(),
                        UserId = rating.UserId ?? "",
                        UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Utilisateur inconnu",
                        UserProfilePictureUrl = user?.ProfilePictureUrl ?? "",
                        Rating = rating.Rating,
                        Comment = comment,
                        CreatedAt = DateTime.UtcNow // Utiliser la date actuelle car le schéma ne stocke pas cette information
                    });
                }

                return bookModel;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> BorrowBookAsync(string bookId, DateTime? dueDate = null)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                    return false;

                // Vérifier si le livre est disponible
                var booksResult = await _supabaseClient.From<DbBook>().Get();
                var book = booksResult.Models.FirstOrDefault(b => b.BookId == bookId);

                if (book == null || !book.AvailabilityStatus?.Equals("Available", StringComparison.OrdinalIgnoreCase) == true)
                    return false;

                // Créer un nouvel emprunt
                var borrowing = new DbBookBorrowing
                {
                    BookId = bookId,
                    UserId = currentUser.Id,
                    BorrowDate = DateTime.UtcNow,
                    DueDate = dueDate ?? DateTime.UtcNow.AddDays(14), // Par défaut, 14 jours
                    IsReturned = false
                };

                await _supabaseClient.From<DbBookBorrowing>().Insert(borrowing);

                // Mettre à jour le statut de disponibilité
                book.AvailabilityStatus = "Borrowed";
                await _supabaseClient.From<DbBook>().Update(book);

                // Mettre à jour les statistiques
                await UpdateBookStatisticsAsync(bookId, "borrow_count", 1);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ReserveBookAsync(string bookId)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                    return false;

                // Vérifier si le livre existe
                var booksResult = await _supabaseClient.From<DbBook>().Get();
                var book = booksResult.Models.FirstOrDefault(b => b.BookId == bookId);

                if (book == null)
                    return false;

                // Vérifier si l'utilisateur a déjà ce livre dans ses intérêts
                var interestsResult = await _supabaseClient.From<DbBookOfInterest>().Get();
                var existingInterest = interestsResult.Models.FirstOrDefault(
                    r => r.BookId == bookId && r.UserId == currentUser.Id
                );

                if (existingInterest != null)
                    return false; // Déjà dans les intérêts

                // Créer un nouvel intérêt
                var interest = new DbBookOfInterest
                {
                    BookId = bookId,
                    UserId = currentUser.Id
                };

                await _supabaseClient.From<DbBookOfInterest>().Insert(interest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CancelReservationAsync(string reservationId)
        {
            try
            {
                // Fetch the reservation from the database
                var reservationsResult = await _supabaseClient.From<DbBookReservation>().Get();
                var reservation = reservationsResult.Models.FirstOrDefault(r => r.ReservationId == reservationId);

                if (reservation == null || reservation.BookId == null)
                {
                    return false; // Reservation not found or BookId is null
                }

                // Delete the reservation
                await _supabaseClient.From<DbBookReservation>().Delete(reservation);

                // Optionally, update the book's availability status
                var booksResult = await _supabaseClient.From<DbBook>().Get();
                var book = booksResult.Models.FirstOrDefault(b => b.BookId == reservation.BookId);

                if (book != null)
                {
                    book.AvailabilityStatus = "Available";
                    await _supabaseClient.From<DbBook>().Update(book);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ReturnBookAsync(string bookId)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                    return false;

                // Trouver l'emprunt actif
                var borrowingsResult = await _supabaseClient.From<DbBookBorrowing>().Get();
                var borrowing = borrowingsResult.Models.FirstOrDefault(
                    b => b.BookId == bookId && b.UserId == currentUser.Id && !b.IsReturned
                );

                if (borrowing == null)
                    return false;

                // Update borrowing directly
                borrowing.IsReturned = true;
                borrowing.ReturnDate = DateTime.UtcNow;
                
                await _supabaseClient.From<DbBookBorrowing>().Update(borrowing);

                // Mettre à jour le statut de disponibilité
                var booksResult = await _supabaseClient.From<DbBook>().Get();
                var book = booksResult.Models.FirstOrDefault(b => b.BookId == bookId);
                    
                if (book != null)
                {
                    book.AvailabilityStatus = "Available";
                    await _supabaseClient.From<DbBook>().Update(book);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RateBookAsync(string bookId, int rating, string? comment = null)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                    return false;

                // Vérifier si l'utilisateur a déjà évalué ce livre
                var ratingsResult = await _supabaseClient.From<DbBookRating>().Get();
                var existingRating = ratingsResult.Models.FirstOrDefault(
                    r => r.BookId == bookId && r.UserId == currentUser.Id
                );

                if (existingRating != null)
                {
                    // Mettre à jour l'évaluation existante
                    existingRating.Rating = rating;
                    await _supabaseClient.From<DbBookRating>().Update(existingRating);
                }
                else
                {
                    // Créer une nouvelle évaluation
                    var newRating = new DbBookRating
                    {
                        BookId = bookId,
                        UserId = currentUser.Id,
                        Rating = rating
                    };

                    await _supabaseClient.From<DbBookRating>().Insert(newRating);
                }

                // Si un commentaire est fourni, l'ajouter ou le mettre à jour
                if (!string.IsNullOrEmpty(comment))
                {
                    // Vérifier si l'utilisateur a déjà commenté ce livre
                    var commentsResult = await _supabaseClient.From<DbBookComment>().Get();
                    var existingComment = commentsResult.Models.FirstOrDefault(
                        c => c.BookId == bookId && c.UserId == currentUser.Id
                    );

                    if (existingComment != null)
                    {
                        // Mettre à jour le commentaire existant
                        existingComment.CommentText = comment;
                        await _supabaseClient.From<DbBookComment>().Update(existingComment);
                    }
                    else
                    {
                        // Créer un nouveau commentaire
                        var newComment = new DbBookComment
                        {
                            BookId = bookId,
                            UserId = currentUser.Id,
                            CommentText = comment,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _supabaseClient.From<DbBookComment>().Insert(newComment);
                        
                        // Mettre à jour le compteur de commentaires
                        await UpdateBookStatisticsAsync(bookId, "comment_count", 1);
                    }
                }

                // Mettre à jour la moyenne des évaluations
                await UpdateBookRatingAverageAsync(bookId);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LikeBookAsync(string bookId)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                    return false;

                // Vérifier si l'utilisateur a déjà aimé ce livre
                var likesResult = await _supabaseClient.From<DbBookLike>().Get();
                var existingLike = likesResult.Models.FirstOrDefault(
                    l => l.BookId == bookId && l.UserId == currentUser.Id
                );

                if (existingLike != null)
                    return true; // Déjà aimé

                // Créer un nouveau "j'aime"
                var like = new DbBookLike
                {
                    BookId = bookId,
                    UserId = currentUser.Id
                };

                await _supabaseClient.From<DbBookLike>().Insert(like);

                // Mettre à jour le compteur de "j'aime"
                await UpdateBookStatisticsAsync(bookId, "like_count", 1);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UnlikeBookAsync(string bookId)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                    return false;

                // Trouver le "j'aime" existant
                var likesResult = await _supabaseClient.From<DbBookLike>().Get();
                var existingLike = likesResult.Models.FirstOrDefault(
                    l => l.BookId == bookId && l.UserId == currentUser.Id
                );

                if (existingLike == null)
                    return true; // Déjà non aimé

                // Supprimer le "j'aime"
                await _supabaseClient.From<DbBookLike>().Delete(existingLike);

                // Mettre à jour le compteur de "j'aime"
                await UpdateBookStatisticsAsync(bookId, "like_count", -1);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Fetches real books from the Supabase database with enhanced details including statistics
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="category">Optional category filter</param>
        /// <param name="searchQuery">Optional search query</param>
        /// <returns>List of book models with detailed information</returns>
        public async Task<List<BookModel>> GetRealBooksAsync(int page = 1, int pageSize = 10, string? category = null, string? searchQuery = null)
        {
            try
            {
                // Adjust pagination values
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 5, 50);
                
                // Fetch books from database with proper filters
                var query = _supabaseClient.From<DbBook>();
                
                // Get all books first and then filter in memory
                var booksResult = await query.Get();
                var filteredBooks = booksResult.Models.AsEnumerable();
                
                // Apply category filter if provided
                if (!string.IsNullOrEmpty(category))
                {
                    filteredBooks = filteredBooks.Where(b => 
                        b.Category != null && b.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
                    );
                }
                
                // Apply search query filter if provided
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    filteredBooks = filteredBooks.Where(b => 
                        (b.Title != null && b.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (b.Author != null && b.Author.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (b.Description != null && b.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (b.ISBN != null && b.ISBN.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    );
                }
                
                // Count total before pagination (only needed for accurate pagination info)
                int totalBooks = filteredBooks.Count();
                
                // Apply pagination
                var pagedBooks = filteredBooks
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                if (!pagedBooks.Any())
                {
                    return new List<BookModel>();
                }
                
                // Get book IDs for batch fetching statistics
                var bookIds = pagedBooks.Where(b => b.BookId != null).Select(b => b.BookId!).ToList();
                
                // Fetch book statistics in a batch query
                var bookStatsResult = await _supabaseClient.From<DbBookStatistics>()
                    .Filter("book_id", Postgrest.Constants.Operator.In, bookIds)
                    .Get();
                
                var statsDict = bookStatsResult.Models
                    .Where(s => s.BookId != null)
                    .ToDictionary(s => s.BookId!, s => s);
                
                // Fetch book likes - to check if current user has liked any books
                var currentUser = await _userService.GetCurrentUserAsync();
                Dictionary<string, bool> userLikes = new Dictionary<string, bool>();
                
                if (currentUser != null)
                {
                    var likesResult = await _supabaseClient.From<DbBookLike>()
                        .Filter("user_id", Postgrest.Constants.Operator.Equals, currentUser.Id)
                        .Filter("book_id", Postgrest.Constants.Operator.In, bookIds)
                        .Get();
                    
                    foreach (var like in likesResult.Models)
                    {
                        if (like.BookId != null)
                        {
                            userLikes[like.BookId] = true;
                        }
                    }
                }
                
                // Map database books to view models with enhanced properties
                var bookModels = new List<BookModel>();
                
                foreach (var book in pagedBooks)
                {
                    if (book?.BookId == null) continue;
                    
                    // Create BookModel with properties from the database
                    var bookModel = new BookModel
                    {
                        Id = book.BookId,
                        Title = book.Title ?? "",
                        Author = book.Author ?? "",
                        ISBN = book.ISBN ?? "",
                        PublicationYear = book.PublicationYear,
                        Publisher = book.Publisher ?? "",
                        Category = book.Category ?? "",
                        Description = book.Description ?? "",
                        // Use the actual cover image URL from database
                        CoverImageUrl = !string.IsNullOrEmpty(book.CoverImageUrl) 
                            ? book.CoverImageUrl 
                            : $"https://via.placeholder.com/150?text={Uri.EscapeDataString(book.Title ?? "Book")}",
                        // Add language from database, defaulting to English if not specified
                        Language = !string.IsNullOrEmpty(book.Language) ? book.Language : "English",
                        // Correction: Vérification insensible à la casse pour le statut
                        AvailableCopies = book.AvailabilityStatus?.Equals("Available", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0,
                        TotalCopies = 1
                    };
                    
                    // Add statistics if available
                    if (statsDict.TryGetValue(book.BookId, out var stats))
                    {
                        bookModel.LikesCount = stats.TotalLikes;
                        bookModel.RatingAverage = stats.AverageRating;
                    }
                    
                    // Set if the current user has liked this book
                    if (currentUser != null && userLikes.ContainsKey(book.BookId))
                    {
                        bookModel.IsLikedByCurrentUser = true;
                    }
                    
                    bookModels.Add(bookModel);
                }
                
                return bookModels;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching real books: {ex.Message}");
                return new List<BookModel>();
            }
        }

        private async Task UpdateBookStatisticsAsync(string bookId, string statField, int increment)
        {
            try
            {
                // Vérifier si les statistiques existent pour ce livre
                var statsResult = await _supabaseClient.From<DbBookStatistics>().Get();
                var stats = statsResult.Models.FirstOrDefault(s => s.BookId == bookId);

                if (stats == null)
                {
                    // Créer de nouvelles statistiques
                    var newStats = new DbBookStatistics
                    {
                        BookId = bookId
                    };

                    // Définir la valeur du champ spécifié
                    switch (statField)
                    {
                        case "borrow_count":
                            newStats.TotalBorrows = increment;
                            break;
                        case "view_count":
                            // Cette colonne n'existe plus dans le schéma, ignorer cette mise à jour
                            break;
                        case "comment_count":
                            // Cette colonne n'existe plus dans le schéma, ignorer cette mise à jour
                            break;
                        case "like_count":
                            newStats.TotalLikes = increment;
                            break;
                    }

                    await _supabaseClient.From<DbBookStatistics>().Insert(newStats);
                }
                else
                {
                    // Mettre à jour les statistiques existantes
                    switch (statField)
                    {
                        case "borrow_count":
                            stats.TotalBorrows = Math.Max(0, stats.TotalBorrows + increment);
                            break;
                        case "view_count":
                            // Cette colonne n'existe plus dans le schéma, ignorer cette mise à jour
                            break;
                        case "comment_count":
                            // Cette colonne n'existe plus dans le schéma, ignorer cette mise à jour
                            break;
                        case "like_count":
                            stats.TotalLikes = Math.Max(0, stats.TotalLikes + increment);
                            break;
                    }

                    await _supabaseClient.From<DbBookStatistics>().Update(stats);
                }
            }
            catch
            {
                // Ignorer les erreurs pour ne pas bloquer le flux principal
            }
        }

        private async Task UpdateBookRatingAverageAsync(string bookId)
        {
            try
            {
                // Récupérer toutes les évaluations pour ce livre
                var ratingsResult = await _supabaseClient.From<DbBookRating>().Get();
                var ratings = ratingsResult.Models.Where(r => r.BookId == bookId).ToList();

                if (ratings.Count > 0)
                {
                    // Calculer la moyenne
                    decimal average = (decimal)ratings.Average(r => r.Rating);

                    // Mettre à jour les statistiques
                    var statsResult = await _supabaseClient.From<DbBookStatistics>().Get();
                    var stats = statsResult.Models.FirstOrDefault(s => s.BookId == bookId);

                    if (stats == null)
                    {
                        // Créer de nouvelles statistiques
                        var newStats = new DbBookStatistics
                        {
                            BookId = bookId,
                            AverageRating = average
                        };

                        await _supabaseClient.From<DbBookStatistics>().Insert(newStats);
                    }
                    else
                    {
                        stats.AverageRating = average;
                        await _supabaseClient.From<DbBookStatistics>().Update(stats);
                    }
                }
            }
            catch
            {
                // Ignorer les erreurs pour ne pas bloquer le flux principal
            }
        }

        private List<BookModel> ConvertToBookModels(List<DbBook> books)
        {
            var bookModels = new List<BookModel>();
            foreach (var book in books)
            {
                if (book?.BookId != null)
                {
                    bookModels.Add(new BookModel
                    {
                        Id = book.BookId,
                        Title = book.Title ?? "",
                        Author = book.Author ?? "",
                        ISBN = book.ISBN ?? "",
                        PublicationYear = book.PublicationYear,
                        Publisher = book.Publisher ?? "",
                        Category = book.Category ?? "",
                        Description = book.Description ?? "",
                        // Use the actual cover image URL from database
                        CoverImageUrl = !string.IsNullOrEmpty(book.CoverImageUrl) 
                            ? book.CoverImageUrl 
                            : $"https://via.placeholder.com/150?text={Uri.EscapeDataString(book.Title ?? "Book")}",
                        // Add language from database, defaulting to English if not specified
                        Language = !string.IsNullOrEmpty(book.Language) ? book.Language : "English",
                        // Correction: Vérification insensible à la casse pour le statut
                        AvailableCopies = book.AvailabilityStatus?.Equals("Available", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0,
                        TotalCopies = 1
                    });
                }
            }
            return bookModels;
        }
    }
}
