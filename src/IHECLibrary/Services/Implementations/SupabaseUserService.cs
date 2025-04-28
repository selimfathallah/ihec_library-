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
                {
                    Console.WriteLine("GetCurrentUserAsync: CurrentUser is null");
                    return null;
                }

                var userId = _supabaseClient.Auth.CurrentUser.Id;
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("GetCurrentUserAsync: CurrentUser.Id is null or empty");
                    return null;
                }

                Console.WriteLine($"GetCurrentUserAsync: Looking up user with ID: {userId}");
                return await GetUserByIdAsync(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCurrentUserAsync Exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<UserModel?> GetUserByIdAsync(string userId)
        {
            try
            {
                Console.WriteLine($"GetUserByIdAsync: Starting lookup for user with ID {userId}");

                // Try to get user data using standard ORM query
                var usersResponse = await _supabaseClient.From<DbUser>()
                    .Where(p => p.UserId == userId)
                    .Get();
                
                // Better handling of query results
                if (usersResponse.Models.Count == 0)
                {
                    Console.WriteLine($"GetUserByIdAsync: No user found with ID {userId} in database");
                    
                    // Try lookup by email instead
                    if (_supabaseClient.Auth.CurrentUser?.Email != null)
                    {
                        var emailQuery = await _supabaseClient.From<DbUser>()
                            .Where(p => p.Email == _supabaseClient.Auth.CurrentUser.Email)
                            .Get();
                        
                        if (emailQuery.Models.Count > 0)
                        {
                            var emailUser = emailQuery.Models.First();
                            Console.WriteLine($"GetUserByIdAsync: Found user by email: {emailUser.Email} with name: {emailUser.FirstName} {emailUser.LastName}");
                            
                            // Create user model from user found by email
                            var emailUserModel = new UserModel
                            {
                                Id = userId,
                                Email = emailUser.Email ?? "",
                                FirstName = emailUser.FirstName ?? "",
                                LastName = emailUser.LastName ?? "",
                                PhoneNumber = emailUser.PhoneNumber ?? "",
                                ProfilePictureUrl = emailUser.ProfilePictureUrl ?? "/Assets/default_profile.png"
                            };
                            
                            return emailUserModel;
                        }
                    }
                    
                    // Failed to find user by either ID or email
                    Console.WriteLine($"GetUserByIdAsync: Failed to find user by ID or email");
                    return null;
                }
                
                // Get the first user from the response
                var user = usersResponse.Models.FirstOrDefault();
                
                if (user == null)
                {
                    Console.WriteLine($"GetUserByIdAsync: User is null despite query returning results");
                    return null;
                }

                // DEBUG: Log values to help diagnose the issue
                Console.WriteLine($"GetUserByIdAsync: Found user with these values:");
                Console.WriteLine($"  - UserId: {user.UserId}");
                Console.WriteLine($"  - Email: {user.Email}");
                Console.WriteLine($"  - FirstName: '{user.FirstName}'");
                Console.WriteLine($"  - LastName: '{user.LastName}'");

                // Create user model with available information
                var userModel = new UserModel
                {
                    Id = user.UserId ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    ProfilePictureUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl) ? 
                        user.ProfilePictureUrl : 
                        "/Assets/default_profile.png"
                };
                
                try 
                {
                    // Attempt to get student profile (non-critical data)
                    var studentProfileResponse = await _supabaseClient.From<DbStudentProfile>()
                        .Where(p => p.StudentId == userId)
                        .Get();
                    
                    var studentProfile = studentProfileResponse.Models.FirstOrDefault();
                    
                    if (studentProfile != null)
                    {
                        userModel.LevelOfStudy = studentProfile.LevelOfStudy;
                        userModel.FieldOfStudy = studentProfile.FieldOfStudy;
                    }
                }
                catch (Exception ex)
                {
                    // Log but continue - this is non-critical data
                    Console.WriteLine($"GetUserByIdAsync: Error getting student profile: {ex.Message}");
                }
                
                try
                {
                    // Attempt to get admin profile (non-critical data)
                    var adminProfileResponse = await _supabaseClient.From<DbAdminProfile>()
                        .Where(p => p.AdminId == userId)
                        .Get();
                    
                    var adminProfile = adminProfileResponse.Models.FirstOrDefault();
                    userModel.IsAdmin = adminProfile != null && adminProfile.IsApproved;
                }
                catch (Exception ex)
                {
                    // Log but continue - this is non-critical data
                    Console.WriteLine($"GetUserByIdAsync: Error getting admin profile: {ex.Message}");
                }

                return userModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserByIdAsync: Exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }
        
        // Helper method to capitalize first letter of a string
        private string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
                
            return char.ToUpper(input[0]) + (input.Length > 1 ? input.Substring(1) : "");
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
                
                if (!string.IsNullOrEmpty(model.ProfilePictureUrl))
                {
                    user.ProfilePictureUrl = model.ProfilePictureUrl;
                }
                
                // Save the changes
                await _supabaseClient.From<DbUser>()
                    .Update(user);

                return true;
            }
            catch
            {
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

                // Obtenir les livres d'intérêt (réservations)
                var booksOfInterest = await _supabaseClient.From<DbBookOfInterest>()
                    .Where(r => r.UserId == userId)
                    .Get();

                // Obtenir les livres aimés
                var likedBooks = await _supabaseClient.From<DbBookLike>()
                    .Where(l => l.UserId == userId)
                    .Get();

                statistics.BorrowedBooksCount = activeBorrowings.Models.Count;
                statistics.ReservedBooksCount = booksOfInterest.Models.Count;
                statistics.LikedBooksCount = likedBooks.Models.Count;
                statistics.Ranking = await GetUserRankingAsync(userId);

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
                            CoverImageUrl = "", 
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
                            CoverImageUrl = "",
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
                            CoverImageUrl = "",
                            AvailableCopies = 0,
                            TotalCopies = 0,
                            LikesCount = 0
                        });
                    }
                }

                return statistics;
            }
            catch
            {
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
