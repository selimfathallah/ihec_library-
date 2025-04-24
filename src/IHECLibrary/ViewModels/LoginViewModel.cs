using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Threading.Tasks;

namespace IHECLibrary.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        public LoginViewModel(INavigationService navigationService, IAuthService authService)
        {
            _navigationService = navigationService;
            _authService = authService;
        }

        [RelayCommand]
        private async Task SignIn()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Veuillez remplir tous les champs";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var result = await _authService.SignInAsync(Email, Password);
                if (result.Success)
                {
                    // Add debug logging
                    Console.WriteLine("Login successful, navigating to Home view");
                    
                    // Clear any error message
                    ErrorMessage = string.Empty;
                    
                    // Ensure we navigate to the Home view
                    await _navigationService.NavigateToAsync("Home");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Échec de la connexion. Veuillez vérifier vos identifiants.";
                    Console.WriteLine($"Login failed: {ErrorMessage}");
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Une erreur s'est produite: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GoogleSignIn()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var result = await _authService.SignInWithGoogleAsync();
                if (result.Success)
                {
                    // Add debug logging
                    Console.WriteLine("Google login successful, navigating to Home view");
                    
                    // Clear any error message
                    ErrorMessage = string.Empty;
                    
                    // Navigate to Home view
                    await _navigationService.NavigateToAsync("Home");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Échec de la connexion avec Google.";
                    Console.WriteLine($"Google login failed: {ErrorMessage}");
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Une erreur s'est produite: {ex.Message}";
                Console.WriteLine($"Google login error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GoToRegister()
        {
            await _navigationService.NavigateToAsync("Register");
        }

        [RelayCommand]
        private async Task GoToAdminLogin()
        {
            await _navigationService.NavigateToAsync("AdminLogin");
        }

        [RelayCommand]
        private async Task ForgotPassword()
        {
            await _navigationService.NavigateToAsync("ForgotPassword");
        }
    }
}
