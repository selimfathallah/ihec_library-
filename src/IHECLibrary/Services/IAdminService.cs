using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IHECLibrary.Services
{
    public interface IAdminService
    {
        Task<AdminModel?> GetCurrentAdminAsync();
        Task<AdminModel?> GetAdminByIdAsync(string adminId);
        Task<List<AdminModel>> GetAllAdminsAsync();
        Task<bool> ApproveAdminRequestAsync(string adminId);
        Task<bool> RejectAdminRequestAsync(string adminId);
        Task<bool> UpdateAdminProfileAsync(AdminProfileUpdateModel model);
        Task<DashboardDataModel> GetDashboardDataAsync();
        Task<List<BookModel>> GetAllBooksAsync();
        Task<bool> AddBookAsync(BookAddModel model);
        Task<bool> UpdateBookAsync(BookUpdateModel model);
        Task<bool> DeleteBookAsync(string bookId);
        Task<List<UserModel>> GetAllUsersAsync();
        Task<List<BorrowingModel>> GetAllBorrowingsAsync();
        Task<List<ReservationModel>> GetAllReservationsAsync();
    }

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

    public class BookUpdateModel : BookAddModel
    {
        public string Id { get; set; } = string.Empty;
    }

    public class BorrowingModel
    {
        public string Id { get; set; } = string.Empty;
        public string BookId { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsReturned { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class ReservationModel
    {
        public string Id { get; set; } = string.Empty;
        public string BookId { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
