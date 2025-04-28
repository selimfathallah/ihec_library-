using System.Threading.Tasks;
using IHECLibrary.Services;

namespace IHECLibrary.Services.Implementations.Mock
{
    public class MockAuthService : IAuthService
    {
        private string _currentUserId = "mock-user-id";

        public Task<AuthResult> SignInAsync(string email, string password)
        {
            var user = new UserModel
            {
                Id = _currentUserId,
                Email = email,
                FirstName = "Mock",
                LastName = "User"
            };

            return Task.FromResult(new AuthResult
            {
                Success = true,
                User = user,
                Message = "Successfully signed in (mock)"
            });
        }

        public Task<AuthResult> SignInWithGoogleAsync()
        {
            var user = new UserModel
            {
                Id = _currentUserId,
                Email = "mockuser@gmail.com",
                FirstName = "Mock",
                LastName = "GoogleUser"
            };

            return Task.FromResult(new AuthResult
            {
                Success = true,
                User = user,
                Message = "Successfully signed in with Google (mock)"
            });
        }

        public Task<AuthResult> RegisterAsync(UserRegistrationModel model)
        {
            var user = new UserModel
            {
                Id = _currentUserId,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                LevelOfStudy = model.LevelOfStudy,
                FieldOfStudy = model.FieldOfStudy
            };

            return Task.FromResult(new AuthResult
            {
                Success = true,
                User = user,
                Message = "Successfully registered (mock)"
            });
        }

        public Task<AuthResult> RegisterAdminAsync(AdminRegistrationModel model)
        {
            var user = new UserModel
            {
                Id = "mock-admin-id",
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                IsAdmin = true
            };

            return Task.FromResult(new AuthResult
            {
                Success = true,
                User = user,
                Message = "Successfully registered admin (mock)"
            });
        }

        public Task<bool> SignOutAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> ResetPasswordAsync(string email)
        {
            return Task.FromResult(true);
        }

        public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            return Task.FromResult(true);
        }

        public bool IsAuthenticated()
        {
            // For mock service, always return true as if the user is authenticated
            return true;
        }
    }
}