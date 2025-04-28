using System.Threading.Tasks;

namespace IHECLibrary.Services
{
    public interface IAuthService
    {
        Task<AuthResult> SignInAsync(string email, string password);
        Task<AuthResult> RegisterAsync(UserRegistrationModel model);
        Task<AuthResult> RegisterAdminAsync(AdminRegistrationModel model);
        Task<AuthResult> SignInWithGoogleAsync();
        Task<bool> SignOutAsync();
        Task<bool> ResetPasswordAsync(string email);
        Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
        
        // Add a new method to check if user is authenticated
        bool IsAuthenticated();
    }
}
