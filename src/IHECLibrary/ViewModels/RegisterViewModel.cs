using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IHECLibrary.ViewModels
{
    public partial class RegisterViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _phoneNumber = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _selectedLevel = string.Empty;

        [ObservableProperty]
        private string _selectedField = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public List<string> LevelOptions { get; } = new List<string>
        {
            "1", "2", "3", "M1", "M2", "Autre"
        };

        public List<string> FieldOptions { get; } = new List<string>
        {
            "BI", "Gestion", "Finance", "Management", "Marketing", "Big Data", "Accounting", "Autre"
        };

        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        public RegisterViewModel(INavigationService navigationService, IAuthService authService)
        {
            _navigationService = navigationService;
            _authService = authService;
            
            // Valeurs par défaut
            SelectedLevel = LevelOptions[0];
            SelectedField = FieldOptions[0];
        }

        [RelayCommand]
        private async Task Register()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(PhoneNumber))
            {
                ErrorMessage = "Veuillez remplir tous les champs obligatoires";
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
                var registrationModel = new UserRegistrationModel
                {
                    Email = Email,
                    Password = Password,
                    FirstName = FirstName,
                    LastName = LastName,
                    PhoneNumber = "+216" + PhoneNumber,
                    LevelOfStudy = SelectedLevel,
                    FieldOfStudy = SelectedField
                };

                var result = await _authService.RegisterAsync(registrationModel);
                if (result.Success)
                {
                    // Add debug logging
                    Console.WriteLine("Registration successful, navigating to Home view");
                    // Clear any error message
                    ErrorMessage = string.Empty;
                    
                    // Ensure we navigate to the Home view
                    await _navigationService.NavigateToAsync("Home");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Échec de l'inscription. Veuillez réessayer.";
                    Console.WriteLine($"Registration failed: {ErrorMessage}");
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
                    await _navigationService.NavigateToAsync("Home");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Échec de la connexion avec Google.";
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
        private async Task GoToLogin()
        {
            await _navigationService.NavigateToAsync("Login");
        }
    }
}
