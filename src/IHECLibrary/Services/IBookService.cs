using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IHECLibrary.Services
{
    public interface IBookService
    {
        Task<List<BookModel>> GetRecommendedBooksAsync();
        Task<List<BookModel>> GetBooksBySearchAsync(string searchQuery);
        Task<List<BookModel>> GetBooksByCategoryAsync(string category);
        Task<BookModel?> GetBookByIdAsync(string id);
        Task<bool> BorrowBookAsync(string bookId, DateTime? dueDate = null);
        Task<bool> ReserveBookAsync(string bookId);
        Task<bool> ReturnBookAsync(string bookId);
        Task<bool> RateBookAsync(string bookId, int rating, string? comment = null);
        Task<bool> LikeBookAsync(string bookId);
        Task<bool> UnlikeBookAsync(string bookId);
        Task<List<BookModel>> GetBooksByFiltersAsync(List<string> categories, bool availableOnly, string? language);
        Task<bool> CancelReservationAsync(string reservationId);
    }

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
        public List<BookRatingModel> Ratings { get; set; } = new List<BookRatingModel>();
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
    }

    public class BookRatingModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserProfilePictureUrl { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
