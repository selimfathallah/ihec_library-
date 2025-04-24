using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IHECLibrary.Services;

namespace IHECLibrary.Services.Implementations.Mock
{
    public class MockBookService : IBookService
    {
        private readonly List<BookModel> _books;
        private readonly Random _random = new Random();

        public MockBookService()
        {
            // Initialize with sample books
            _books = new List<BookModel>
            {
                // Finance books
                new BookModel
                {
                    Id = "1",
                    Title = "Principles of Finance",
                    Author = "Mohamed Ben Salah",
                    Description = "A comprehensive introduction to the principles of finance and their practical applications.",
                    Category = "Finance",
                    Language = "English",
                    PublicationYear = 2022,
                    TotalCopies = 5,
                    AvailableCopies = 3,
                    LikesCount = 42,
                    CoverImageUrl = "",
                    ISBN = "978-0123456789",
                    Publisher = "IHEC Press",
                    Ratings = new List<BookRatingModel>()
                },
                new BookModel
                {
                    Id = "2",
                    Title = "Les fondements de l'investissement",
                    Author = "Leila Trabelsi",
                    Description = "Une analyse détaillée des stratégies d'investissement dans les marchés tunisiens.",
                    Category = "Finance",
                    Language = "French",
                    PublicationYear = 2021,
                    TotalCopies = 3,
                    AvailableCopies = 0,
                    LikesCount = 28,
                    CoverImageUrl = "",
                    ISBN = "978-0123456790",
                    Publisher = "IHEC Press",
                    Ratings = new List<BookRatingModel>()
                },

                // Management books
                new BookModel
                {
                    Id = "3",
                    Title = "Strategic Management",
                    Author = "Amine Koubaa",
                    Description = "Explores strategic management concepts with case studies from North African companies.",
                    Category = "Management",
                    Language = "English",
                    PublicationYear = 2023,
                    TotalCopies = 8,
                    AvailableCopies = 5,
                    LikesCount = 36,
                    CoverImageUrl = "",
                    ISBN = "978-0123456791",
                    Publisher = "IHEC Press",
                    Ratings = new List<BookRatingModel>()
                },
                new BookModel
                {
                    Id = "4",
                    Title = "Leadership in MENA Region",
                    Author = "Fatima El-Mansouri",
                    Description = "Analysis of leadership styles and their effectiveness in the MENA business environment.",
                    Category = "Management",
                    Language = "English",
                    PublicationYear = 2020,
                    TotalCopies = 4,
                    AvailableCopies = 2,
                    LikesCount = 31,
                    CoverImageUrl = "",
                    ISBN = "",
                    Publisher = "",
                    Ratings = new List<BookRatingModel>()
                },

                // Marketing books
                new BookModel
                {
                    Id = "5",
                    Title = "Digital Marketing for MENA Markets",
                    Author = "Nadia Belhaj",
                    Description = "A practical guide to digital marketing strategies for businesses in the MENA region.",
                    Category = "Marketing",
                    Language = "English",
                    PublicationYear = 2023,
                    TotalCopies = 6,
                    AvailableCopies = 3,
                    LikesCount = 45,
                    CoverImageUrl = "",
                    ISBN = "",
                    Publisher = "",
                    Ratings = new List<BookRatingModel>()
                },
                new BookModel
                {
                    Id = "6",
                    Title = "Consumer Behavior in North Africa",
                    Author = "Tarek Masmoudi",
                    Description = "Research-based insights into consumer behavior patterns across North African markets.",
                    Category = "Marketing",
                    Language = "English",
                    PublicationYear = 2021,
                    TotalCopies = 5,
                    AvailableCopies = 1,
                    LikesCount = 37,
                    CoverImageUrl = "",
                    ISBN = "",
                    Publisher = "",
                    Ratings = new List<BookRatingModel>()
                },

                // Economics books
                new BookModel
                {
                    Id = "7",
                    Title = "Macroeconomics: A Tunisian Perspective",
                    Author = "Hichem Ben Ammar",
                    Description = "In-depth analysis of macroeconomic principles with a focus on the Tunisian economy.",
                    Category = "Economics",
                    Language = "English",
                    PublicationYear = 2022,
                    TotalCopies = 7,
                    AvailableCopies = 4,
                    LikesCount = 39,
                    CoverImageUrl = "",
                    ISBN = "",
                    Publisher = "",
                    Ratings = new List<BookRatingModel>()
                },
                new BookModel
                {
                    Id = "8",
                    Title = "Économie du développement",
                    Author = "Sarah Lahmar",
                    Description = "Étude approfondie des théories du développement économique appliquées au contexte tunisien.",
                    Category = "Economics",
                    Language = "French",
                    PublicationYear = 2020,
                    TotalCopies = 4,
                    AvailableCopies = 0,
                    LikesCount = 30,
                    CoverImageUrl = "",
                    ISBN = "",
                    Publisher = "",
                    Ratings = new List<BookRatingModel>()
                },

                // Accounting books
                new BookModel
                {
                    Id = "9",
                    Title = "Financial Accounting Standards",
                    Author = "Ahmed Khelifi",
                    Description = "Comprehensive guide to financial accounting standards with practical examples.",
                    Category = "Accounting",
                    Language = "English",
                    PublicationYear = 2023,
                    TotalCopies = 6,
                    AvailableCopies = 4,
                    LikesCount = 33,
                    CoverImageUrl = "",
                    ISBN = "",
                    Publisher = "",
                    Ratings = new List<BookRatingModel>()
                },
                new BookModel
                {
                    Id = "10",
                    Title = "Comptabilité analytique",
                    Author = "Rim Jaziri",
                    Description = "Manuel pratique de comptabilité analytique pour les étudiants et professionnels.",
                    Category = "Accounting",
                    Language = "French",
                    PublicationYear = 2021,
                    TotalCopies = 5,
                    AvailableCopies = 2,
                    LikesCount = 29,
                    CoverImageUrl = "",
                    ISBN = "",
                    Publisher = "",
                    Ratings = new List<BookRatingModel>()
                },

                // BI books
                new BookModel
                {
                    Id = "11",
                    Title = "Business Intelligence for Decision Making",
                    Author = "Karim Mansour",
                    Description = "Practical approaches to using business intelligence for strategic decision making.",
                    Category = "BI",
                    Language = "English",
                    PublicationYear = 2023,
                    TotalCopies = 7,
                    AvailableCopies = 3,
                    LikesCount = 47,
                    CoverImageUrl = "",
                    ISBN = "",
                    Publisher = "",
                    Ratings = new List<BookRatingModel>()
                },
                new BookModel
                {
                    Id = "12",
                    Title = "Data Visualization Techniques",
                    Author = "Yasmine Triki",
                    Description = "Comprehensive guide to effective data visualization for business applications.",
                    Category = "BI",
                    Language = "English",
                    PublicationYear = 2022,
                    TotalCopies = 5,
                    AvailableCopies = 2,
                    LikesCount = 41,
                    CoverImageUrl = "",
                    ISBN = "",
                    Publisher = "",
                    Ratings = new List<BookRatingModel>()
                },

                // Big Data books
                new BookModel
                {
                    Id = "13",
                    Title = "Big Data Analytics in Finance",
                    Author = "Omar El Ghazouani",
                    Description = "Advanced techniques for applying big data analytics in financial institutions.",
                    Category = "Big Data",
                    Language = "English",
                    PublicationYear = 2023,
                    TotalCopies = 6,
                    AvailableCopies = 3,
                    LikesCount = 52,
                    CoverImageUrl = "",
                    ISBN = "",
                    Publisher = "",
                    Ratings = new List<BookRatingModel>()
                },
                new BookModel
                {
                    Id = "14",
                    Title = "Machine Learning pour l'analyse de données",
                    Author = "Sami Bouaziz",
                    Description = "Introduction aux techniques de machine learning appliquées à l'analyse de grands volumes de données.",
                    Category = "Big Data",
                    Language = "French",
                    PublicationYear = 2022,
                    TotalCopies = 4,
                    AvailableCopies = 0,
                    LikesCount = 38,
                    CoverImageUrl = "",
                    ISBN = "",
                    Publisher = "",
                    Ratings = new List<BookRatingModel>()
                },
                new BookModel
                {
                    Id = "15",
                    Title = "Data Privacy and Ethics",
                    Author = "Amira Ben Salah",
                    Description = "Exploring the ethical considerations and privacy challenges in the age of big data.",
                    Category = "Big Data",
                    Language = "English",
                    PublicationYear = 2023,
                    TotalCopies = 5,
                    AvailableCopies = 4,
                    LikesCount = 35,
                    CoverImageUrl = "",
                    ISBN = "",
                    Publisher = "",
                    Ratings = new List<BookRatingModel>()
                }
            };
        }

        // Get a list of all books
        public Task<List<BookModel>> GetAllBooksAsync()
        {
            return Task.FromResult(_books.ToList());
        }

        // Get books based on search query
        public Task<List<BookModel>> GetBooksBySearchAsync(string searchQuery)
        {
            var result = _books.Where(b => 
                b.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                b.Category.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                b.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
            ).ToList();
            
            return Task.FromResult(result);
        }

        // Get books by category
        public Task<List<BookModel>> GetBooksByCategoryAsync(string category)
        {
            var result = _books.Where(b => 
                b.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
            ).ToList();
            
            return Task.FromResult(result);
        }

        // Get books based on multiple filters
        public Task<List<BookModel>> GetBooksByFiltersAsync(List<string> categories, bool availableOnly, string? language)
        {
            var result = _books.Where(b => 
                (categories.Count == 0 || categories.Contains(b.Category)) &&
                (!availableOnly || b.AvailableCopies > 0) &&
                (string.IsNullOrEmpty(language) || b.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
            ).ToList();
            
            return Task.FromResult(result);
        }

        /// <summary>
        /// Fetches real books from the mock database with pagination and filtering
        /// </summary>
        public Task<List<BookModel>> GetRealBooksAsync(int page = 1, int pageSize = 10, string? category = null, string? searchQuery = null)
        {
            // Apply filters
            var filteredBooks = _books.AsEnumerable();
            
            // Apply category filter if provided
            if (!string.IsNullOrEmpty(category))
            {
                filteredBooks = filteredBooks.Where(b => 
                    b.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
                );
            }
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                filteredBooks = filteredBooks.Where(b => 
                    b.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    b.Author.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    b.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    (b.ISBN != null && b.ISBN.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                );
            }
            
            // Apply pagination
            var result = filteredBooks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            return Task.FromResult(result);
        }

        // Get recommended books (personalized or popular)
        public Task<List<BookModel>> GetRecommendedBooksAsync()
        {
            // For demo purposes, return 4 random books as recommendations
            var recommendations = _books
                .OrderBy(_ => _random.Next()) // Random ordering
                .Take(4)
                .ToList();
            
            return Task.FromResult(recommendations);
        }

        // Check if a book is available
        public Task<bool> IsBookAvailableAsync(string bookId)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            return Task.FromResult(book != null && book.AvailableCopies > 0);
        }

        // Borrow a book (reducing available copies)
        public Task<bool> BorrowBookAsync(string bookId, DateTime? dueDate = null)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book != null && book.AvailableCopies > 0)
            {
                book.AvailableCopies--;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        // Return a book (increasing available copies)
        public Task<bool> ReturnBookAsync(string bookId)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book != null && book.AvailableCopies < book.TotalCopies)
            {
                book.AvailableCopies++;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        // Reserve a book that is not currently available
        public Task<bool> ReserveBookAsync(string bookId)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            return Task.FromResult(book != null);
        }

        // Get user's borrowed books
        public Task<List<BookModel>> GetBorrowedBooksAsync(string userId)
        {
            // For mock purposes, return 2 random books as "borrowed"
            var borrowed = _books
                .OrderBy(_ => _random.Next())
                .Take(2)
                .ToList();
            
            return Task.FromResult(borrowed);
        }

        // Get user's reserved books
        public Task<List<BookModel>> GetReservedBooksAsync(string userId)
        {
            // For mock purposes, return 1 random book as "reserved"
            var reserved = _books
                .OrderBy(_ => _random.Next())
                .Take(1)
                .ToList();
            
            return Task.FromResult(reserved);
        }

        // Get book details by ID
        public Task<BookModel?> GetBookByIdAsync(string id)
        {
            var book = _books.FirstOrDefault(b => b.Id == id);
            return Task.FromResult(book);
        }

        // Add a book to user's favorites/likes
        public Task<bool> LikeBookAsync(string bookId)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book != null)
            {
                book.LikesCount++;
                book.IsLikedByCurrentUser = true;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        // Remove a book from user's favorites/likes
        public Task<bool> UnlikeBookAsync(string bookId)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book != null)
            {
                if (book.LikesCount > 0)
                    book.LikesCount--;
                book.IsLikedByCurrentUser = false;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        // Rate a book
        public Task<bool> RateBookAsync(string bookId, int rating, string? comment = null)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book != null && rating >= 1 && rating <= 5)
            {
                // Create a new rating
                var bookRating = new BookRatingModel
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = "current-user", // In a real implementation, this would be the actual user ID
                    UserName = "Current User", // In a real implementation, this would be the actual user's name
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };
                
                // Add to book's ratings
                book.Ratings.Add(bookRating);
                
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }

        // Cancel a reservation
        public Task<bool> CancelReservationAsync(string reservationId)
        {
            // Since this is a mock service and we don't have actual reservation objects,
            // we'll simply return true to simulate successful cancellation
            return Task.FromResult(true);
        }
    }
}