using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Postgrest.Attributes;
using Postgrest.Models;
using Postgrest;
using Postgrest.Responses;
using IHECLibrary.Services.Models;

namespace IHECLibrary.Services.Implementations
{
    public class SupabaseAdminService : IAdminService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly IAuthService _authService;

        public SupabaseAdminService(Supabase.Client supabaseClient, IAuthService authService)
        {
            _supabaseClient = supabaseClient;
            _authService = authService;
        }

        public async Task<AdminModel?> GetCurrentAdminAsync()
        {
            try
            {
                if (_supabaseClient.Auth.CurrentUser == null)
                    return null;

                var userId = _supabaseClient.Auth.CurrentUser.Id;
                // Fix null reference warning
                return userId != null ? await GetAdminByIdAsync(userId) : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<AdminModel?> GetAdminByIdAsync(string adminId)
        {
            try
            {
                var adminProfile = await _supabaseClient.From<AdminProfile>()
                    .Where(p => p.AdminId == adminId && p.IsApproved)
                    .Single();

                if (adminProfile != null)
                {
                    return new AdminModel
                    {
                        Id = adminProfile.AdminId ?? "",
                        Email = _supabaseClient.Auth.CurrentUser?.Email ?? "",
                        FirstName = adminProfile.FirstName ?? "",
                        LastName = adminProfile.LastName ?? "",
                        PhoneNumber = adminProfile.PhoneNumber ?? "",
                        JobTitle = adminProfile.JobTitle ?? "",
                        ProfilePictureUrl = adminProfile.ProfilePictureUrl ?? "",
                        IsApproved = adminProfile.IsApproved,
                        CreatedAt = adminProfile.CreatedAt
                    };
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<AdminModel>> GetAllAdminsAsync()
        {
            try
            {
                var adminProfiles = await _supabaseClient.From<AdminProfile>()
                    .Get();

                var admins = new List<AdminModel>();
                var profilesList = adminProfiles.Models;
                
                foreach (var profile in profilesList)
                {
                    admins.Add(new AdminModel
                    {
                        Id = profile.AdminId ?? "",
                        FirstName = profile.FirstName ?? "",
                        LastName = profile.LastName ?? "",
                        PhoneNumber = profile.PhoneNumber ?? "",
                        JobTitle = profile.JobTitle ?? "",
                        ProfilePictureUrl = profile.ProfilePictureUrl ?? "",
                        IsApproved = profile.IsApproved,
                        CreatedAt = profile.CreatedAt
                    });
                }

                return admins;
            }
            catch
            {
                return new List<AdminModel>();
            }
        }

        public async Task<bool> ApproveAdminRequestAsync(string adminId)
        {
            try
            {
                await _supabaseClient.From<AdminProfile>()
                    .Where(p => p.AdminId == adminId)
                    .Set(p => p.IsApproved, true)
                    .Update();

                // Ajouter une activité dans le journal
                await AddActivityAsync("Approbation d'administrateur", $"Un nouvel administrateur a été approuvé", "Register");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RejectAdminRequestAsync(string adminId)
        {
            try
            {
                // Supprimer le profil administrateur
                await _supabaseClient.From<AdminProfile>()
                    .Where(p => p.AdminId == adminId)
                    .Delete();

                // Ajouter une activité dans le journal
                await AddActivityAsync("Rejet d'administrateur", $"Une demande d'administrateur a été rejetée", "Register");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAdminProfileAsync(AdminProfileUpdateModel model)
        {
            try
            {
                if (_supabaseClient.Auth.CurrentUser == null)
                    return false;

                var adminId = _supabaseClient.Auth.CurrentUser.Id;
                var admin = new AdminProfile
                {
                    AdminId = adminId,
                    FirstName = model.FirstName ?? "",
                    LastName = model.LastName ?? "",
                    PhoneNumber = model.PhoneNumber ?? "",
                    JobTitle = model.JobTitle ?? ""
                };

                if (!string.IsNullOrEmpty(model.ProfilePictureUrl))
                {
                    admin.ProfilePictureUrl = model.ProfilePictureUrl;
                }

                await _supabaseClient.From<AdminProfile>()
                    .Where(p => p.AdminId == adminId)
                    .Update(admin);

                // Ajouter une activité dans le journal
                await AddActivityAsync("Mise à jour de profil admin", "Un administrateur a mis à jour son profil", "UpdateProfile");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<DashboardDataModel> GetDashboardDataAsync()
        {
            try
            {
                var dashboardData = new DashboardDataModel();

                // Get total books count
                var books = await _supabaseClient.From<DbBook>().Get();
                dashboardData.TotalBooksCount = books.Models.Count;

                // Get total users count
                var users = await _supabaseClient.From<UserProfile>().Get();
                dashboardData.TotalUsersCount = users.Models.Count;

                // Get active borrowings count
                var activeBorrowings = await _supabaseClient.From<DbBookBorrowingDetails>()
                    .Where(b => !b.IsReturned)
                    .Get();
                dashboardData.ActiveBorrowingsCount = activeBorrowings.Models.Count;

                // Get pending reservations count
                var pendingReservations = await _supabaseClient.From<DbReservation>()
                    .Where(r => r.Status == "pending")
                    .Get();
                dashboardData.PendingReservationsCount = pendingReservations.Models.Count;

                // Get recent activities
                var recentActivities = await _supabaseClient.From<Activity>()
                    .Order("created_at", Postgrest.Constants.Ordering.Descending)
                    .Limit(10)
                    .Get();

                foreach (var activity in recentActivities.Models)
                {
                    if (activity.Id != null)
                    {
                        dashboardData.RecentActivities.Add(new ActivityModel
                        {
                            Id = activity.Id,
                            Title = activity.Title ?? "",
                            Description = activity.Description ?? "",
                            Type = activity.Type ?? "",
                            Time = FormatTimeAgo(activity.CreatedAt),
                            CreatedAt = activity.CreatedAt
                        });
                    }
                }

                // Get popular books
                var popularBooks = await _supabaseClient.From<DbBook>()
                    .Order("likes_count", Postgrest.Constants.Ordering.Descending)
                    .Limit(5)
                    .Get();

                foreach (var book in popularBooks.Models)
                {
                    if (book.BookId != null)
                    {
                        // Count borrowings for this book
                        var bookBorrowings = await _supabaseClient.From<DbBookBorrowingDetails>()
                            .Where(b => b.BookId == book.BookId)
                            .Get();

                        dashboardData.PopularBooks.Add(new PopularBookModel
                        {
                            Id = book.BookId,
                            Title = book.Title ?? "",
                            Author = book.Author ?? "",
                            BorrowCount = bookBorrowings.Models.Count
                        });
                    }
                }

                // Get active users
                var activeUsers = users.Models
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToList();

                foreach (var user in activeUsers)
                {
                    if (user.Id != null)
                    {
                        // Count active borrowings for this user
                        var userBorrowings = await _supabaseClient.From<DbBookBorrowingDetails>()
                            .Where(b => b.UserId == user.Id && !b.IsReturned)
                            .Get();

                        dashboardData.ActiveUsers.Add(new ActiveUserModel
                        {
                            Id = user.Id,
                            FirstName = user.FirstName ?? "",
                            LastName = user.LastName ?? "",
                            Email = await GetUserEmailAsync(user.Id),
                            ProfilePictureUrl = user.ProfilePictureUrl ?? "",
                            BorrowedBooksCount = userBorrowings.Models.Count
                        });
                    }
                }

                return dashboardData;
            }
            catch
            {
                return new DashboardDataModel();
            }
        }

        public async Task<List<BookModel>> GetAllBooksAsync()
        {
            try
            {
                var books = await _supabaseClient.From<DbBook>()
                    .Get();

                var bookModels = new List<BookModel>();
                foreach (var book in books.Models)
                {
                    string bookId = book.BookId ?? "";
                    string title = book.Title ?? "";
                    string author = book.Author ?? "";
                    string isbn = book.ISBN ?? "";
                    int publicationYear = book.PublicationYear;
                    string publisher = book.Publisher ?? "";
                    string category = book.Category ?? "";
                    string description = book.Description ?? "";
                    
                    // Get these properties using GetValue to work around compiler issues
                    string coverImageUrl = "";
                    int availableCopies = 0;
                    int totalCopies = 0;
                    int likesCount = 0;
                    
                    try {
                        // Try to get values using reflection as a workaround
                        var coverImageProperty = typeof(DbBook).GetProperty("CoverImageUrl");
                        if (coverImageProperty != null) 
                            coverImageUrl = (coverImageProperty.GetValue(book) as string) ?? "";
                            
                        var availableCopiesProperty = typeof(DbBook).GetProperty("AvailableCopies");
                        if (availableCopiesProperty != null) 
                            availableCopies = (int)(availableCopiesProperty.GetValue(book) ?? 0);
                            
                        var totalCopiesProperty = typeof(DbBook).GetProperty("TotalCopies");
                        if (totalCopiesProperty != null) 
                            totalCopies = (int)(totalCopiesProperty.GetValue(book) ?? 0);
                            
                        var likesCountProperty = typeof(DbBook).GetProperty("LikesCount");
                        if (likesCountProperty != null) 
                            likesCount = (int)(likesCountProperty.GetValue(book) ?? 0);
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Error accessing book properties: {ex.Message}");
                    }

                    bookModels.Add(new BookModel
                    {
                        Id = bookId,
                        Title = title,
                        Author = author,
                        ISBN = isbn,
                        PublicationYear = publicationYear,
                        Publisher = publisher,
                        Category = category,
                        Description = description,
                        CoverImageUrl = coverImageUrl,
                        AvailableCopies = availableCopies,
                        TotalCopies = totalCopies,
                        LikesCount = likesCount
                    });
                }

                return bookModels;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting books: {ex.Message}");
                return new List<BookModel>();
            }
        }

        public async Task<bool> AddBookAsync(BookAddModel model)
        {
            try
            {
                // Create book with basic properties that compile correctly
                var book = new DbBook
                {
                    BookId = Guid.NewGuid().ToString(),
                    Title = model.Title,
                    Author = model.Author,
                    ISBN = model.ISBN,
                    PublicationYear = model.PublicationYear,
                    Publisher = model.Publisher,
                    Category = model.Category,
                    Description = model.Description
                };

                // Set additional properties using reflection to avoid compiler errors
                try 
                {
                    // Try to set values using reflection as a workaround
                    typeof(DbBook).GetProperty("CoverImageUrl")?.SetValue(book, model.CoverImageUrl ?? "");
                    typeof(DbBook).GetProperty("AvailableCopies")?.SetValue(book, model.TotalCopies);
                    typeof(DbBook).GetProperty("TotalCopies")?.SetValue(book, model.TotalCopies);
                    typeof(DbBook).GetProperty("LikesCount")?.SetValue(book, 0);
                    typeof(DbBook).GetProperty("Language")?.SetValue(book, "Français");
                    typeof(DbBook).GetProperty("CreatedAt")?.SetValue(book, DateTime.UtcNow);
                    typeof(DbBook).GetProperty("UpdatedAt")?.SetValue(book, DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting book properties: {ex.Message}");
                }

                await _supabaseClient.From<DbBook>().Insert(book);

                // Ajouter une activité dans le journal
                await AddActivityAsync("Ajout de livre", $"Le livre '{model.Title}' a été ajouté à la bibliothèque", "AddBook");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding book: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateBookAsync(BookUpdateModel model)
        {
            try
            {
                // Vérifier si le livre existe
                var existingBook = await _supabaseClient.From<DbBook>()
                    .Where(b => b.BookId == model.Id)
                    .Single();

                if (existingBook == null)
                    return false;

                // Get total copies using reflection if direct property access fails
                int existingTotalCopies = 0;
                try {
                    var totalCopiesProperty = typeof(DbBook).GetProperty("TotalCopies");
                    if (totalCopiesProperty != null)
                        existingTotalCopies = (int)(totalCopiesProperty.GetValue(existingBook) ?? 0);
                } 
                catch (Exception ex) {
                    Console.WriteLine($"Error getting TotalCopies: {ex.Message}");
                }

                // Calculate difference in copies
                int copyDifference = model.TotalCopies - existingTotalCopies;
                
                // Get available copies using reflection
                int existingAvailableCopies = 0;
                try {
                    var availableCopiesProperty = typeof(DbBook).GetProperty("AvailableCopies");
                    if (availableCopiesProperty != null) {
                        var value = availableCopiesProperty.GetValue(existingBook);
                        if (value != null)
                            existingAvailableCopies = Convert.ToInt32(value);
                    }
                } 
                catch (Exception ex) {
                    Console.WriteLine($"Error getting AvailableCopies: {ex.Message}");
                }
                
                // Get cover image URL using reflection
                string existingCoverImageUrl = "";
                try {
                    var coverImageProperty = typeof(DbBook).GetProperty("CoverImageUrl");
                    if (coverImageProperty != null)
                        existingCoverImageUrl = (coverImageProperty.GetValue(existingBook) as string) ?? "";
                } 
                catch (Exception ex) {
                    Console.WriteLine($"Error getting CoverImageUrl: {ex.Message}");
                }

                // Create a new book object with basic properties
                var updatedBook = new DbBook
                {
                    BookId = model.Id,
                    Title = model.Title ?? "",
                    Author = model.Author ?? "",
                    ISBN = model.ISBN ?? "",
                    PublicationYear = model.PublicationYear,
                    Publisher = model.Publisher ?? "",
                    Category = model.Category ?? "",
                    Description = model.Description ?? ""
                };

                // Set additional properties using reflection
                try {
                    // Set total copies
                    typeof(DbBook).GetProperty("TotalCopies")?.SetValue(updatedBook, model.TotalCopies);
                    
                    // Set available copies with the calculated difference
                    typeof(DbBook).GetProperty("AvailableCopies")?.SetValue(updatedBook, existingAvailableCopies + copyDifference);
                    
                    // Set updated date
                    typeof(DbBook).GetProperty("UpdatedAt")?.SetValue(updatedBook, DateTime.UtcNow);
                    
                    // Set cover image URL based on whether a new one was provided
                    string coverImageUrl = !string.IsNullOrEmpty(model.CoverImageUrl) ? 
                        model.CoverImageUrl : existingCoverImageUrl;
                    typeof(DbBook).GetProperty("CoverImageUrl")?.SetValue(updatedBook, coverImageUrl);
                } 
                catch (Exception ex) {
                    Console.WriteLine($"Error setting book properties: {ex.Message}");
                }

                // Update the book
                await _supabaseClient.From<DbBook>()
                    .Where(b => b.BookId == model.Id)
                    .Update(updatedBook);

                // Ajouter une activité dans le journal
                await AddActivityAsync("Mise à jour de livre", $"Le livre '{model.Title}' a été mis à jour", "UpdateBook");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating book: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteBookAsync(string bookId)
        {
            try
            {
                // Vérifier si le livre existe
                var existingBook = await _supabaseClient.From<DbBook>()
                    .Where(b => b.BookId == bookId)
                    .Single();

                if (existingBook == null)
                    return false;

                // Vérifier s'il y a des emprunts actifs pour ce livre
                var activeBorrowings = await _supabaseClient.From<DbBookBorrowingDetails>()
                    .Where(b => b.BookId == bookId && !b.IsReturned)
                    .Get();

                if (activeBorrowings.Models.Count > 0)
                    return false; // Ne pas supprimer un livre avec des emprunts actifs

                // Supprimer les réservations pour ce livre
                await _supabaseClient.From<DbReservation>()
                    .Where(r => r.BookId == bookId)
                    .Delete();

                // Supprimer les évaluations pour ce livre
                await _supabaseClient.From<DbBookRating>()
                    .Where(r => r.BookId == bookId)
                    .Delete();

                // Supprimer les "j'aime" pour ce livre
                await _supabaseClient.From<DbBookLike>()
                    .Where(l => l.BookId == bookId)
                    .Delete();

                // Supprimer le livre
                await _supabaseClient.From<DbBook>()
                    .Where(b => b.BookId == bookId)
                    .Delete();

                // Ajouter une activité dans le journal
                await AddActivityAsync("Suppression de livre", $"Le livre '{existingBook.Title ?? ""}' a été supprimé", "DeleteBook");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            try
            {
                var userProfiles = await _supabaseClient.From<UserProfile>()
                    .Get();

                var users = new List<UserModel>();
                foreach (var profile in userProfiles.Models)
                {
                    users.Add(new UserModel
                    {
                        Id = profile.Id ?? "",
                        Email = await GetUserEmailAsync(profile.Id ?? ""),
                        FirstName = profile.FirstName ?? "",
                        LastName = profile.LastName ?? "",
                        PhoneNumber = profile.PhoneNumber ?? "",
                        LevelOfStudy = profile.LevelOfStudy ?? "",
                        FieldOfStudy = profile.FieldOfStudy ?? "",
                        ProfilePictureUrl = profile.ProfilePictureUrl ?? "",
                        IsAdmin = profile.IsAdmin
                    });
                }

                return users;
            }
            catch
            {
                return new List<UserModel>();
            }
        }

        public async Task<List<BorrowingModel>> GetAllBorrowingsAsync()
        {
            try
            {
                var borrowings = await _supabaseClient.From<DbBookBorrowingDetails>()
                    .Order("borrow_date", Postgrest.Constants.Ordering.Descending)
                    .Get();

                var borrowingModels = new List<BorrowingModel>();
                foreach (var borrowing in borrowings.Models)
                {
                    // Obtenir les détails du livre
                    var book = await _supabaseClient.From<DbBook>()
                        .Where(b => b.BookId == borrowing.BookId)
                        .Single();

                    // Obtenir les détails de l'utilisateur
                    var user = await _supabaseClient.From<UserProfile>()
                        .Where(u => u.Id == borrowing.UserId)
                        .Single();

                    if (book != null && user != null)
                    {
                        borrowingModels.Add(new BorrowingModel
                        {
                            Id = borrowing.Id ?? "",
                            BookId = borrowing.BookId ?? "",
                            BookTitle = book.Title ?? "",
                            UserId = borrowing.UserId ?? "",
                            UserName = $"{user.FirstName ?? ""} {user.LastName ?? ""}",
                            BorrowDate = borrowing.BorrowDate,
                            DueDate = borrowing.DueDate,
                            ReturnDate = borrowing.ReturnDate,
                            IsReturned = borrowing.IsReturned,
                            IsOverdue = !borrowing.IsReturned && borrowing.DueDate < DateTime.UtcNow
                        });
                    }
                }

                return borrowingModels;
            }
            catch
            {
                return new List<BorrowingModel>();
            }
        }

        public async Task<List<ReservationModel>> GetAllReservationsAsync()
        {
            try
            {
                var reservations = await _supabaseClient.From<DbReservation>()
                    .Order("reservation_date", Postgrest.Constants.Ordering.Descending)
                    .Get();

                var reservationModels = new List<ReservationModel>();
                foreach (var reservation in reservations.Models)
                {
                    // Obtenir les détails du livre
                    var book = await _supabaseClient.From<DbBook>()
                        .Where(b => b.BookId == reservation.BookId)
                        .Single();

                    // Obtenir les détails de l'utilisateur
                    var user = await _supabaseClient.From<UserProfile>()
                        .Where(u => u.Id == reservation.UserId)
                        .Single();

                    if (book != null && user != null)
                    {
                        reservationModels.Add(new ReservationModel
                        {
                            Id = reservation.Id ?? "",
                            BookId = reservation.BookId ?? "",
                            BookTitle = book.Title ?? "",
                            UserId = reservation.UserId ?? "",
                            UserName = $"{user.FirstName ?? ""} {user.LastName ?? ""}",
                            ReservationDate = reservation.ReservationDate,
                            Status = reservation.Status ?? ""
                        });
                    }
                }

                return reservationModels;
            }
            catch
            {
                return new List<ReservationModel>();
            }
        }

        private async Task<bool> AddActivityAsync(string title, string description, string type)
        {
            try
            {
                var activity = new Activity
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = title,
                    Description = description,
                    Type = type,
                    CreatedAt = DateTime.UtcNow
                };

                await _supabaseClient.From<Activity>().Insert(activity);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> GetUserEmailAsync(string userId)
        {
            try
            {
                // Cette méthode est une approximation car nous n'avons pas accès direct aux emails des utilisateurs
                // Dans une implémentation réelle, vous devriez utiliser l'API d'administration de Supabase
                var user = await _supabaseClient.From<User>()
                    .Where(u => u.Id == userId)
                    .Single();

                return user?.Email ?? "";
            }
            catch
            {
                return "";
            }
        }

        private string FormatTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "À l'instant";
            if (timeSpan.TotalMinutes < 60)
                return $"Il y a {(int)timeSpan.TotalMinutes} minute(s)";
            if (timeSpan.TotalHours < 24)
                return $"Il y a {(int)timeSpan.TotalHours} heure(s)";
            if (timeSpan.TotalDays < 30)
                return $"Il y a {(int)timeSpan.TotalDays} jour(s)";

            return dateTime.ToString("dd/MM/yyyy");
        }
    }
}
