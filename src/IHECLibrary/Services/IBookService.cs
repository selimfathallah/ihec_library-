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
        
        /// <summary>
        /// Fetches real books from the Supabase database with enhanced details including statistics
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="category">Optional category filter</param>
        /// <param name="searchQuery">Optional search query</param>
        /// <returns>List of book models with detailed information</returns>
        Task<List<BookModel>> GetRealBooksAsync(int page = 1, int pageSize = 10, string? category = null, string? searchQuery = null);
    }
}
