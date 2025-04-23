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

    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Message { get; set; } // Added Message property for success messages
        public UserModel? User { get; set; }
    }

    public class UserRegistrationModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string LevelOfStudy { get; set; } = string.Empty;
        public string FieldOfStudy { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    }

    public class AdminRegistrationModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
    }

    public class UserModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? LevelOfStudy { get; set; }
        public string? FieldOfStudy { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsAdmin { get; set; }
        public string? JobTitle { get; set; }
        public bool IsApproved { get; set; }
        public bool IsBlocked { get; set; }
    }
}
