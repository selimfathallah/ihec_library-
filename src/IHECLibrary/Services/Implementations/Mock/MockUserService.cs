using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IHECLibrary.Services;

namespace IHECLibrary.Services.Implementations.Mock
{
    public class MockUserService : IUserService
    {
        private readonly UserModel _currentUser = new UserModel
        {
            Id = "mock-user-id",
            Email = "user@example.com",
            FirstName = "Mock",
            LastName = "User",
            PhoneNumber = "12345678",
            LevelOfStudy = "Bachelor",
            FieldOfStudy = "Finance"
        };

        private readonly List<UserModel> _users = new List<UserModel>();

        public MockUserService()
        {
            // Add the current user to the users list
            _users.Add(_currentUser);
            
            // Add some additional mock users
            _users.Add(new UserModel
            {
                Id = "user-1",
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "98765432",
                LevelOfStudy = "Master",
                FieldOfStudy = "Marketing"
            });
            
            _users.Add(new UserModel
            {
                Id = "user-2",
                Email = "jane.smith@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                PhoneNumber = "45678901",
                LevelOfStudy = "PhD",
                FieldOfStudy = "Economics"
            });
        }

        public Task<UserModel?> GetCurrentUserAsync()
        {
            return Task.FromResult<UserModel?>(_currentUser);
        }

        public Task<UserModel?> GetUserByIdAsync(string userId)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            return Task.FromResult<UserModel?>(user);
        }

        public Task<List<UserModel>> SearchUsersAsync(string searchQuery)
        {
            var results = _users.Where(u => 
                u.Email.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                u.FirstName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
            ).ToList();
            
            return Task.FromResult(results);
        }

        public Task<bool> UpdateUserProfileAsync(UserProfileUpdateModel model)
        {
            _currentUser.FirstName = model.FirstName;
            _currentUser.LastName = model.LastName;
            _currentUser.PhoneNumber = model.PhoneNumber;
            _currentUser.LevelOfStudy = model.LevelOfStudy;
            _currentUser.FieldOfStudy = model.FieldOfStudy;
            _currentUser.ProfilePictureUrl = model.ProfilePictureUrl;
            
            return Task.FromResult(true);
        }

        public Task<string> GetUserRankingAsync(string userId)
        {
            return Task.FromResult("Gold");
        }

        public Task<UserStatisticsModel> GetUserStatisticsAsync(string userId)
        {
            var stats = new UserStatisticsModel
            {
                BorrowedBooksCount = 5,
                ReservedBooksCount = 2,
                LikedBooksCount = 10,
                Ranking = "Gold",
                BorrowedBooks = new List<BookModel>
                {
                    new BookModel
                    {
                        Id = "1",
                        Title = "Principles of Finance",
                        Author = "John Smith"
                    },
                    new BookModel
                    {
                        Id = "2",
                        Title = "Marketing Strategies",
                        Author = "Jane Johnson"
                    }
                },
                ReservedBooks = new List<BookModel>
                {
                    new BookModel
                    {
                        Id = "3",
                        Title = "Economics 101",
                        Author = "Robert Williams"
                    }
                },
                LikedBooks = new List<BookModel>
                {
                    new BookModel
                    {
                        Id = "4", 
                        Title = "Business Analytics",
                        Author = "Lisa Brown"
                    }
                }
            };
            
            return Task.FromResult(stats);
        }
    }
}