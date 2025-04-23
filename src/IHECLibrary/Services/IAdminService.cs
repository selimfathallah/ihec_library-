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
}
