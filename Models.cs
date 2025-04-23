using System;
using System.Collections.Generic;

namespace IHECLibrary
{
    // Modèles pour les livres
    public class BookModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public string Publisher { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public int AvailableCopies { get; set; }
        public int TotalCopies { get; set; }
        public int LikesCount { get; set; }
        public string Language { get; set; } = string.Empty;
    }

    // Modèles pour les utilisateurs
    public class UserModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? LevelOfStudy { get; set; }
        public string? FieldOfStudy { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsAdmin { get; set; }
    }

    // Modèles pour l'authentification
    public class UserRegistrationModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? LevelOfStudy { get; set; }
        public string? FieldOfStudy { get; set; }
    }

    public class AdminRegistrationModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public UserModel? User { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Message { get; set; }
    }

    // Modèles pour l'administration
    public class AdminModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminProfileUpdateModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    }

    public class DashboardDataModel
    {
        public int TotalBooksCount { get; set; }
        public int TotalUsersCount { get; set; }
        public int ActiveBorrowingsCount { get; set; }
        public int PendingReservationsCount { get; set; }
        public List<ActivityModel> RecentActivities { get; set; } = new List<ActivityModel>();
        public List<PopularBookModel> PopularBooks { get; set; } = new List<PopularBookModel>();
        public List<ActiveUserModel> ActiveUsers { get; set; } = new List<ActiveUserModel>();
    }

    public class ActivityModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class PopularBookModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int BorrowCount { get; set; }
    }

    public class ActiveUserModel
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public int BorrowedBooksCount { get; set; }
    }

    public class BookAddModel
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public string Publisher { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public int TotalCopies { get; set; }
    }

    public class BookUpdateModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public string Publisher { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public int TotalCopies { get; set; }
    }

    // Modèles pour le chatbot
    public class ChatbotResponseModel
    {
        public string Message { get; set; } = string.Empty;
        public List<string>? Suggestions { get; set; }
        public List<BookModel>? BookRecommendations { get; set; }
    }
}
