using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Threading.Tasks;

namespace IHECLibrary.ViewModels
{
    public partial class AdminLoginViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _rememberMe;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        public AdminLoginViewModel(INavigationService navigationService, IAuthService authService)
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

            if (!Email.EndsWith("@ihec.ucar.tn"))
            {
                ErrorMessage = "Veuillez utiliser votre email de l'IHEC (@ihec.ucar.tn)";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var result = await _authService.SignInAsync(Email, Password);
                if (result.Success)
                {
                    if (result.User?.IsAdmin == true)
                    {
                        await _navigationService.NavigateToAsync("AdminDashboard");
                    }
                    else
                    {
                        ErrorMessage = "Vous n'avez pas les droits d'administrateur.";
                    }
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Échec de la connexion. Veuillez vérifier vos identifiants.";
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
        private async Task GoToAdminRegister()
        {
            await _navigationService.NavigateToAsync("AdminRegister");
        }

        [RelayCommand]
        private async Task GoToUserLogin()
        {
            await _navigationService.NavigateToAsync("Login");
        }

        [RelayCommand]
        private async Task ForgotPassword()
        {
            await _navigationService.NavigateToAsync("ForgotPassword");
        }
    }
}
