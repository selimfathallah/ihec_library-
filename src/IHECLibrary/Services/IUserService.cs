using System.Collections.Generic;
using System.Threading.Tasks;

namespace IHECLibrary.Services
{
    public interface IUserService
    {
        Task<UserModel?> GetCurrentUserAsync();
        Task<UserModel?> GetUserByIdAsync(string userId);
        Task<List<UserModel>> SearchUsersAsync(string searchQuery);
        Task<bool> UpdateUserProfileAsync(UserProfileUpdateModel model);
        Task<string> GetUserRankingAsync(string userId);
        Task<UserStatisticsModel> GetUserStatisticsAsync(string userId);
    }

    public class UserProfileUpdateModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? LevelOfStudy { get; set; }
        public string? FieldOfStudy { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class UserStatisticsModel
    {
        public int BorrowedBooksCount { get; set; }
        public int ReservedBooksCount { get; set; }
        public int LikedBooksCount { get; set; }
        public string Ranking { get; set; } = string.Empty;
        public List<BookModel> BorrowedBooks { get; set; } = new List<BookModel>();
        public List<BookModel> ReservedBooks { get; set; } = new List<BookModel>();
        public List<BookModel> LikedBooks { get; set; } = new List<BookModel>();
    }
}
