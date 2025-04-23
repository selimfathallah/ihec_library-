using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IHECLibrary.ViewModels
{
    public partial class AdminRegisterViewModel : ViewModelBase
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
        private string _selectedJobTitle = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public List<string> JobTitleOptions { get; } = new List<string>
        {
            "Professor", "Librarian", "Administration"
        };

        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        public AdminRegisterViewModel(INavigationService navigationService, IAuthService authService)
        {
            _navigationService = navigationService;
            _authService = authService;
            
            // Valeur par défaut
            SelectedJobTitle = JobTitleOptions[0];
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
                var registrationModel = new AdminRegistrationModel
                {
                    Email = Email,
                    Password = Password,
                    FirstName = FirstName,
                    LastName = LastName,
                    PhoneNumber = "+216" + PhoneNumber,
                    JobTitle = SelectedJobTitle
                };

                var result = await _authService.RegisterAdminAsync(registrationModel);
                if (result.Success)
                {
                    // Rediriger vers une page de confirmation d'attente d'approbation
                    await _navigationService.NavigateToAsync("AdminRegistrationPending");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Échec de l'inscription. Veuillez réessayer.";
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
        private async Task GoToAdminLogin()
        {
            await _navigationService.NavigateToAsync("AdminLogin");
        }
    }
}
