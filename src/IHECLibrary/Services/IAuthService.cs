using System.Threading.Tasks;

namespace IHECLibrary.Services
{
    public interface IAuthService
    {
        Task<AuthResult> SignInAsync(string email, string password);
        Task<AuthResult> SignInWithGoogleAsync();
        Task<AuthResult> RegisterAsync(UserRegistrationModel model);
        Task<AuthResult> RegisterAdminAsync(AdminRegistrationModel model);
        Task<bool> SignOutAsync();
        Task<bool> ResetPasswordAsync(string email);
        Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
    }
}
