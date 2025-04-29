using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IHECLibrary.Services;

namespace IHECLibrary.Services.Implementations.Mock
{
    public class MockAdminService : IAdminService
    {
        private readonly AdminModel _currentAdmin = new AdminModel
        {
            Id = "mock-admin-id",
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            PhoneNumber = "12345678",
            JobTitle = "Library Manager",
            IsApproved = true,
            CreatedAt = DateTime.Now.AddMonths(-3)
        };

        private readonly List<AdminModel> _admins = new List<AdminModel>();
        private readonly List<UserModel> _users = new List<UserModel>();
        private readonly List<BookModel> _books = new List<BookModel>();
        private readonly List<BorrowingModel> _borrowings = new List<BorrowingModel>();
        private readonly List<ReservationModel> _reservations = new List<ReservationModel>();

        public MockAdminService()
        {
            // Initialize admin data
            _admins.Add(_currentAdmin);
            _admins.Add(new AdminModel
            {
                Id = "admin-2",
                Email = "admin2@example.com",
                FirstName = "Jane",
                LastName = "Admin",
                PhoneNumber = "87654321",
                JobTitle = "Assistant Librarian",
                IsApproved = true,
                CreatedAt = DateTime.Now.AddMonths(-2)
            });
            _admins.Add(new AdminModel
            {
                Id = "pending-admin",
                Email = "pending@example.com",
                FirstName = "Pending",
                LastName = "Admin",
                PhoneNumber = "55554444",
                JobTitle = "Library Assistant",
                IsApproved = false,
                CreatedAt = DateTime.Now.AddDays(-5)
            });

            // Initialize user data
            _users.Add(new UserModel
            {
                Id = "user-1",
                Email = "user1@example.com",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "11112222",
                LevelOfStudy = "Bachelor",
                FieldOfStudy = "Finance"
            });
            _users.Add(new UserModel
            {
                Id = "user-2",
                Email = "user2@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                PhoneNumber = "33334444",
                LevelOfStudy = "Master",
                FieldOfStudy = "Marketing"
            });

            // Initialize book data
            _books.Add(new BookModel
            {
                Id = "book-1",
                Title = "Principles of Finance",
                Author = "John Smith",
                ISBN = "1234567890",
                PublicationYear = 2022,
                Category = "Finance",
                Language = "English",
                AvailableCopies = 3,
                TotalCopies = 5
            });
            _books.Add(new BookModel
            {
                Id = "book-2",
                Title = "Marketing Strategies",
                Author = "Jane Johnson",
                ISBN = "0987654321",
                PublicationYear = 2021,
                Category = "Marketing",
                Language = "English",
                AvailableCopies = 0,
                TotalCopies = 2
            });

            // Initialize borrowing data
            _borrowings.Add(new BorrowingModel
            {
                Id = "borrow-1",
                BookId = "book-1",
                BookTitle = "Principles of Finance",
                UserId = "user-1",
                UserName = "John Doe",
                BorrowDate = DateTime.Now.AddDays(-14),
                DueDate = DateTime.Now.AddDays(7),
                IsReturned = false
            });
            _borrowings.Add(new BorrowingModel
            {
                Id = "borrow-2",
                BookId = "book-2",
                BookTitle = "Marketing Strategies",
                UserId = "user-2",
                UserName = "Jane Smith",
                BorrowDate = DateTime.Now.AddDays(-7),
                DueDate = DateTime.Now.AddDays(-1),
                IsReturned = false,
                IsOverdue = true
            });

            // Initialize reservation data
            _reservations.Add(new ReservationModel
            {
                Id = "res-1",
                BookId = "book-2",
                BookTitle = "Marketing Strategies",
                UserId = "user-1",
                UserName = "John Doe",
                ReservationDate = DateTime.Now.AddDays(-3),
                Status = "Pending"
            });
        }

        public Task<AdminModel?> GetCurrentAdminAsync()
        {
            return Task.FromResult<AdminModel?>(_currentAdmin);
        }

        public Task<AdminModel?> GetAdminByIdAsync(string adminId)
        {
            var admin = _admins.FirstOrDefault(a => a.Id == adminId);
            return Task.FromResult<AdminModel?>(admin);
        }

        public Task<List<AdminModel>> GetAllAdminsAsync()
        {
            return Task.FromResult(_admins);
        }

        public Task<bool> ApproveAdminRequestAsync(string adminId)
        {
            var admin = _admins.FirstOrDefault(a => a.Id == adminId);
            if (admin != null)
            {
                admin.IsApproved = true;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> RejectAdminRequestAsync(string adminId)
        {
            var admin = _admins.FirstOrDefault(a => a.Id == adminId);
            if (admin != null)
            {
                _admins.Remove(admin);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> UpdateAdminProfileAsync(AdminProfileUpdateModel model)
        {
            // Use null-conditional operator to prevent null reference warnings
            _currentAdmin.FirstName = model.FirstName ?? _currentAdmin.FirstName;
            _currentAdmin.LastName = model.LastName ?? _currentAdmin.LastName;
            _currentAdmin.PhoneNumber = model.PhoneNumber ?? _currentAdmin.PhoneNumber;
            _currentAdmin.JobTitle = model.JobTitle ?? _currentAdmin.JobTitle;
            _currentAdmin.ProfilePictureUrl = model.ProfilePictureUrl ?? _currentAdmin.ProfilePictureUrl;
            
            return Task.FromResult(true);
        }

        public Task<DashboardDataModel> GetDashboardDataAsync()
        {
            var dashboard = new DashboardDataModel
            {
                TotalBooksCount = _books.Count,
                TotalUsersCount = _users.Count,
                ActiveBorrowingsCount = _borrowings.Count(b => !b.IsReturned),
                PendingReservationsCount = _reservations.Count(r => r.Status == "Pending"),
                RecentActivities = new List<ActivityModel>
                {
                    new ActivityModel
                    {
                        Id = "act-1",
                        Title = "New Book Added",
                        Description = "Principles of Finance was added to the library",
                        Type = "book_added",
                        CreatedAt = DateTime.Now.AddDays(-1)
                    },
                    new ActivityModel
                    {
                        Id = "act-2",
                        Title = "Book Borrowed",
                        Description = "Marketing Strategies was borrowed by Jane Smith",
                        Type = "book_borrowed",
                        CreatedAt = DateTime.Now.AddDays(-7)
                    }
                },
                PopularBooks = new List<PopularBookModel>
                {
                    new PopularBookModel
                    {
                        Id = "book-1",
                        Title = "Principles of Finance",
                        Author = "John Smith",
                        BorrowCount = 15
                    },
                    new PopularBookModel
                    {
                        Id = "book-2",
                        Title = "Marketing Strategies",
                        Author = "Jane Johnson",
                        BorrowCount = 12
                    }
                },
                ActiveUsers = new List<ActiveUserModel>
                {
                    new ActiveUserModel
                    {
                        Id = "user-1",
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "user1@example.com",
                        BorrowedBooksCount = 5
                    },
                    new ActiveUserModel
                    {
                        Id = "user-2",
                        FirstName = "Jane",
                        LastName = "Smith",
                        Email = "user2@example.com",
                        BorrowedBooksCount = 3
                    }
                }
            };
            
            return Task.FromResult(dashboard);
        }

        public Task<List<BookModel>> GetAllBooksAsync()
        {
            return Task.FromResult(_books);
        }

        public Task<bool> AddBookAsync(BookAddModel model)
        {
            var book = new BookModel
            {
                Id = "book-" + (_books.Count + 1),
                Title = model.Title,
                Author = model.Author,
                ISBN = model.ISBN,
                PublicationYear = model.PublicationYear,
                Publisher = model.Publisher,
                Category = model.Category,
                Description = model.Description,
                CoverImageUrl = model.CoverImageUrl ?? "",
                TotalCopies = model.TotalCopies,
                AvailableCopies = model.TotalCopies
            };
            
            _books.Add(book);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateBookAsync(BookUpdateModel model)
        {
            var book = _books.FirstOrDefault(b => b.Id == model.Id);
            if (book != null)
            {
                book.Title = model.Title;
                book.Author = model.Author;
                book.ISBN = model.ISBN;
                book.PublicationYear = model.PublicationYear;
                book.Publisher = model.Publisher ?? book.Publisher;
                book.Category = model.Category;
                book.Description = model.Description ?? book.Description;
                book.CoverImageUrl = model.CoverImageUrl ?? book.CoverImageUrl;
                
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }

        public Task<bool> DeleteBookAsync(string bookId)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book != null)
            {
                _books.Remove(book);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }

        public Task<List<UserModel>> GetAllUsersAsync()
        {
            return Task.FromResult(_users);
        }

        public Task<List<BorrowingModel>> GetAllBorrowingsAsync()
        {
            return Task.FromResult(_borrowings);
        }

        public Task<List<ReservationModel>> GetAllReservationsAsync()
        {
            return Task.FromResult(_reservations);
        }
    }
}