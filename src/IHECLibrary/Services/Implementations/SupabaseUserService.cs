using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Postgrest.Attributes;
using Postgrest.Models;
using Postgrest.Responses;
using IHECLibrary.Services.Models; // Import the correct models namespace

namespace IHECLibrary.Services.Implementations
{
    public class SupabaseUserService : IUserService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly IAuthService _authService;

        public SupabaseUserService(Supabase.Client supabaseClient, IAuthService authService)
        {
            _supabaseClient = supabaseClient;
            _authService = authService;
        }

        public async Task<UserModel?> GetCurrentUserAsync()
        {
            try
            {
                if (_supabaseClient.Auth.CurrentUser == null)
                    return null;

                var userId = _supabaseClient.Auth.CurrentUser.Id;
                if (string.IsNullOrEmpty(userId))
                    return null;

                return await GetUserByIdAsync(userId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<UserModel?> GetUserByIdAsync(string userId)
        {
            try
            {
                // Récupérer les informations de base de l'utilisateur
                var user = await _supabaseClient.From<DbUser>()
                    .Where(p => p.UserId == userId)
                    .Single();

                if (user == null)
                    return null;

                // Vérifier si l'utilisateur est un étudiant
                var studentProfile = await _supabaseClient.From<DbStudentProfile>()
                    .Where(p => p.StudentId == userId)
                    .Single();

                // Vérifier si l'utilisateur est un administrateur
                var adminProfile = await _supabaseClient.From<DbAdminProfile>()
                    .Where(p => p.AdminId == userId)
                    .Single();

                // Créer le modèle utilisateur avec les informations disponibles
                var userModel = new UserModel
                {
                    Id = user.UserId ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    IsAdmin = adminProfile != null && adminProfile.IsApproved
                };

                // Ajouter les informations spécifiques aux étudiants si disponibles
                if (studentProfile != null)
                {
                    userModel.LevelOfStudy = studentProfile.LevelOfStudy;
                    userModel.FieldOfStudy = studentProfile.FieldOfStudy;
                }

                return userModel;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<UserModel>> SearchUsersAsync(string searchQuery)
        {
            try
            {
                var users = await _supabaseClient.From<DbUser>()
                    .Where(p => (p.FirstName != null && p.FirstName.Contains(searchQuery)) || 
                                (p.LastName != null && p.LastName.Contains(searchQuery)))
                    .Get();

                var userModels = new List<UserModel>();
                foreach (var user in users.Models)
                {
                    if (user == null || user.UserId == null)
                        continue;

                    // Vérifier si l'utilisateur est un étudiant
                    var studentProfile = await _supabaseClient.From<DbStudentProfile>()
                        .Where(p => p.StudentId == user.UserId)
                        .Single();

                    // Vérifier si l'utilisateur est un administrateur
                    var adminProfile = await _supabaseClient.From<DbAdminProfile>()
                        .Where(p => p.AdminId == user.UserId)
                        .Single();

                    var userModel = new UserModel
                    {
                        Id = user.UserId ?? "",
                        Email = user.Email ?? "",
                        FirstName = user.FirstName ?? "",
                        LastName = user.LastName ?? "",
                        PhoneNumber = user.PhoneNumber ?? "",
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        IsAdmin = adminProfile != null && adminProfile.IsApproved
                    };

                    // Ajouter les informations spécifiques aux étudiants si disponibles
                    if (studentProfile != null)
                    {
                        userModel.LevelOfStudy = studentProfile.LevelOfStudy;
                        userModel.FieldOfStudy = studentProfile.FieldOfStudy;
                    }

                    userModels.Add(userModel);
                }

                return userModels;
            }
            catch
            {
                return new List<UserModel>();
            }
        }

        public async Task<bool> UpdateUserProfileAsync(UserProfileUpdateModel model)
        {
            try
            {
                if (_supabaseClient.Auth.CurrentUser == null)
                    return false;

                var userId = _supabaseClient.Auth.CurrentUser.Id;
                if (string.IsNullOrEmpty(userId))
                    return false;
                
                // Get the current user data
                var user = await _supabaseClient.From<DbUser>()
                    .Where(p => p.UserId == userId)
                    .Single();
                
                if (user == null)
                    return false;
                
                // Update the properties
                user.FirstName = model.FirstName ?? "";
                user.LastName = model.LastName ?? "";
                user.PhoneNumber = model.PhoneNumber ?? "";
                
                // Handle profile picture update if provided
                if (model.HasNewProfilePicture && !string.IsNullOrEmpty(model.ProfilePictureData))
                {
                    try
                    {
                        Console.WriteLine("Uploading new profile picture...");
                        
                        // Create a unique filename for the profile picture
                        string fileName = $"profile_{userId}_{DateTime.UtcNow.Ticks}.jpg";
                        
                        // Convert base64 string to bytes
                        byte[] imageData = Convert.FromBase64String(model.ProfilePictureData);
                        
                        // Upload to Supabase Storage
                        var upload = await _supabaseClient.Storage
                            .From("profile-pictures")
                            .Upload(
                                imageData,
                                fileName,
                                new Supabase.Storage.FileOptions 
                                { 
                                    CacheControl = "3600", 
                                    Upsert = true
                                });
                        
                        if (!string.IsNullOrEmpty(upload))
                        {
                            // Get the public URL
                            string profilePictureUrl = _supabaseClient.Storage
                                .From("profile-pictures")
                                .GetPublicUrl(fileName);
                            
                            Console.WriteLine($"Profile picture uploaded successfully: {profilePictureUrl}");
                            
                            // Update user's profile picture URL
                            user.ProfilePictureUrl = profilePictureUrl;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error uploading profile picture: {ex.Message}");
                        // Continue with the update even if image upload fails
                    }
                }
                else if (!string.IsNullOrEmpty(model.ProfilePictureUrl))
                {
                    user.ProfilePictureUrl = model.ProfilePictureUrl;
                }
                
                // Save the changes to user table
                await _supabaseClient.From<DbUser>()
                    .Update(user);
                
                // Update student profile information if needed
                try
                {
                    var studentProfile = await _supabaseClient.From<DbStudentProfile>()
                        .Where(p => p.StudentId == userId)
                        .Single();
                        
                    if (studentProfile != null)
                    {
                        if (!string.IsNullOrEmpty(model.LevelOfStudy))
                            studentProfile.LevelOfStudy = model.LevelOfStudy;
                            
                        if (!string.IsNullOrEmpty(model.FieldOfStudy))
                            studentProfile.FieldOfStudy = model.FieldOfStudy;
                            
                        await _supabaseClient.From<DbStudentProfile>()
                            .Update(studentProfile);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating student profile: {ex.Message}");
                    // Continue even if this part fails
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateUserProfileAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetUserRankingAsync(string userId)
        {
            try
            {
                // Obtenir le nombre de livres empruntés par l'utilisateur
                var borrowings = await _supabaseClient.From<DbBookBorrowing>()
                    .Where(b => b.UserId == userId)
                    .Get();

                int borrowedBooksCount = borrowings.Models.Count;

                // Déterminer le rang en fonction du nombre de livres empruntés
                if (borrowedBooksCount > 10)
                    return "Master";
                else if (borrowedBooksCount >= 5)
                    return "Gold";
                else if (borrowedBooksCount >= 2)
                    return "Silver";
                else
                    return "Bronze";
            }
            catch
            {
                return "Bronze"; // Rang par défaut en cas d'erreur
            }
        }

        public async Task<UserStatisticsModel> GetUserStatisticsAsync(string userId)
        {
            try
            {
                var statistics = new UserStatisticsModel();

                // Obtenir les emprunts actifs
                var activeBorrowings = await _supabaseClient.From<DbBookBorrowing>()
                    .Where(b => b.UserId == userId && !b.IsReturned)
                    .Get();

                // Obtenir les emprunts historiques (tous les emprunts, y compris retournés)
                var allBorrowings = await _supabaseClient.From<DbBookBorrowing>()
                    .Where(b => b.UserId == userId)
                    .Get();

                // Obtenir les livres d'intérêt (réservations)
                var booksOfInterest = await _supabaseClient.From<DbBookOfInterest>()
                    .Where(r => r.UserId == userId)
                    .Get();

                // Obtenir les livres aimés
                var likedBooks = await _supabaseClient.From<DbBookLike>()
                    .Where(l => l.UserId == userId)
                    .Get();

                // Obtenir le profil étudiant pour vérifier si le rang est déjà enregistré
                var studentProfile = await _supabaseClient.From<DbStudentProfile>()
                    .Where(p => p.StudentId == userId)
                    .Single();

                statistics.BorrowedBooksCount = activeBorrowings.Models.Count;
                statistics.ReservedBooksCount = booksOfInterest.Models.Count;
                statistics.LikedBooksCount = likedBooks.Models.Count;
                
                // Déterminer le rang en fonction du nombre total de livres empruntés historiquement
                int totalBorrowedBooks = allBorrowings.Models.Count;
                string ranking;
                
                if (totalBorrowedBooks > 10)
                    ranking = "Master";
                else if (totalBorrowedBooks >= 5)
                    ranking = "Gold";
                else if (totalBorrowedBooks >= 2)
                    ranking = "Silver";
                else
                    ranking = "Bronze";
                
                statistics.Ranking = ranking;
                
                // Si un profil étudiant existe, mettre à jour le rang dans la base de données
                if (studentProfile != null && !string.IsNullOrEmpty(studentProfile.StudentId))
                {
                    studentProfile.Ranking = ranking;
                    await _supabaseClient.From<DbStudentProfile>()
                        .Where(p => p.StudentId == userId)
                        .Update(studentProfile);
                }

                // Récupérer les détails des livres empruntés
                foreach (var borrowing in activeBorrowings.Models)
                {
                    if (borrowing?.BookId == null)
                        continue;

                    var book = await _supabaseClient.From<DbBook>()
                        .Where(b => b.BookId == borrowing.BookId)
                        .Single();

                    if (book != null)
                    {
                        statistics.BorrowedBooks.Add(new BookModel
                        {
                            Id = book.BookId ?? "",
                            Title = book.Title ?? "",
                            Author = book.Author ?? "",
                            ISBN = book.ISBN ?? "",
                            PublicationYear = book.PublicationYear,
                            Publisher = book.Publisher ?? "",
                            Category = book.Category ?? "",
                            Description = book.Description ?? "",
                            CoverImageUrl = book.CoverImageUrl,
                            AvailableCopies = 0,
                            TotalCopies = 0,
                            LikesCount = 0
                        });
                    }
                }

                // Récupérer les détails des livres d'intérêt
                foreach (var bookOfInterest in booksOfInterest.Models)
                {
                    if (bookOfInterest?.BookId == null)
                        continue;

                    var book = await _supabaseClient.From<DbBook>()
                        .Where(b => b.BookId == bookOfInterest.BookId)
                        .Single();

                    if (book != null)
                    {
                        statistics.ReservedBooks.Add(new BookModel
                        {
                            Id = book.BookId ?? "",
                            Title = book.Title ?? "",
                            Author = book.Author ?? "",
                            ISBN = book.ISBN ?? "",
                            PublicationYear = book.PublicationYear,
                            Publisher = book.Publisher ?? "",
                            Category = book.Category ?? "",
                            Description = book.Description ?? "",
                            CoverImageUrl = book.CoverImageUrl,
                            AvailableCopies = 0,
                            TotalCopies = 0,
                            LikesCount = 0
                        });
                    }
                }

                // Récupérer les détails des livres aimés
                foreach (var like in likedBooks.Models)
                {
                    if (like?.BookId == null)
                        continue;

                    var book = await _supabaseClient.From<DbBook>()
                        .Where(b => b.BookId == like.BookId)
                        .Single();

                    if (book != null)
                    {
                        statistics.LikedBooks.Add(new BookModel
                        {
                            Id = book.BookId ?? "",
                            Title = book.Title ?? "",
                            Author = book.Author ?? "",
                            ISBN = book.ISBN ?? "",
                            PublicationYear = book.PublicationYear,
                            Publisher = book.Publisher ?? "",
                            Category = book.Category ?? "",
                            Description = book.Description ?? "",
                            CoverImageUrl = book.CoverImageUrl,
                            AvailableCopies = 0,
                            TotalCopies = 0,
                            LikesCount = 0
                        });
                    }
                }

                return statistics;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user statistics: {ex.Message}");
                return new UserStatisticsModel
                {
                    Ranking = "Bronze" // Rang par défaut en cas d'erreur
                };
            }
        }
    }

    // Only keep the user-specific model classes here
    // All book-related database models are now defined in DatabaseModels.cs

    [Table("users")]
    public class DbUser : BaseModel
    {
        [PrimaryKey("user_id")]
        public string? UserId { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }
    }

    [Table("student_profiles")]
    public class DbStudentProfile : BaseModel
    {
        [PrimaryKey("student_id")]
        public string? StudentId { get; set; }

        [Column("level_of_study")]
        public string? LevelOfStudy { get; set; }

        [Column("field_of_study")]
        public string? FieldOfStudy { get; set; }
        
        [Column("ranking")]
        public string? Ranking { get; set; }
    }

    [Table("admin_profiles")]
    public class DbAdminProfile : BaseModel
    {
        [PrimaryKey("admin_id")]
        public string? AdminId { get; set; }

        [Column("job_title")]
        public string? JobTitle { get; set; }

        [Column("is_approved")]
        public bool IsApproved { get; set; }

        [Column("approved_by")]
        public string? ApprovedBy { get; set; }
    }
}
