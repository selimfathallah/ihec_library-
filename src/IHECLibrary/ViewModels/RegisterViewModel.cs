using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;

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

        [ObservableProperty]
        private string _profilePicturePath = string.Empty;

        [ObservableProperty]
        private Bitmap? _profilePicturePreview;

        [ObservableProperty]
        private bool _hasProfilePicture;

        [ObservableProperty]
        private string _profilePictureDisplay = "Select Profile Picture";

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
        private readonly Window? _parentWindow;

        public RegisterViewModel(INavigationService navigationService, IAuthService authService)
        {
            _navigationService = navigationService;
            _authService = authService;
            
            // Get parent window for file picker
            _parentWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
            
            // Valeurs par défaut
            SelectedLevel = LevelOptions[0];
            SelectedField = FieldOptions[0];
        }

        [RelayCommand]
        private async Task SelectProfilePicture()
        {
            if (_parentWindow == null)
            {
                ErrorMessage = "Cannot open file picker (window not available)";
                return;
            }

            try
            {
                // Create file picker options for images
                var options = new FilePickerOpenOptions
                {
                    Title = "Select Profile Picture",
                    AllowMultiple = false,
                    FileTypeFilter = new FilePickerFileType[]
                    {
                        new FilePickerFileType("Image Files")
                        {
                            Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.gif" },
                            MimeTypes = new[] { "image/jpeg", "image/png", "image/gif" }
                        }
                    }
                };

                // Open file picker
                var result = await _parentWindow.StorageProvider.OpenFilePickerAsync(options);
                
                if (result != null && result.Count > 0)
                {
                    // Get the selected file
                    var file = result[0];
                    ProfilePicturePath = file.Path.LocalPath;
                    ProfilePictureDisplay = Path.GetFileName(ProfilePicturePath);
                    HasProfilePicture = true;

                    // Load the image preview
                    await LoadProfilePreview(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error selecting profile picture: {ex.Message}");
                ErrorMessage = "Error selecting image file";
            }
        }

        private async Task LoadProfilePreview(IStorageFile file)
        {
            try
            {
                using var stream = await file.OpenReadAsync();
                ProfilePicturePreview = new Bitmap(stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image preview: {ex.Message}");
                ProfilePicturePreview = null;
            }
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
                Console.WriteLine("Creating registration model");
                
                // Prepare profile picture data if available
                string profilePictureBase64 = string.Empty;
                if (HasProfilePicture && !string.IsNullOrEmpty(ProfilePicturePath))
                {
                    try
                    {
                        byte[] imageBytes = File.ReadAllBytes(ProfilePicturePath);
                        profilePictureBase64 = Convert.ToBase64String(imageBytes);
                        Console.WriteLine("Profile picture data prepared");
                    }
                    catch (Exception imgEx)
                    {
                        Console.WriteLine($"Error reading profile picture: {imgEx.Message}");
                        // Continue without profile picture
                    }
                }
                
                var registrationModel = new UserRegistrationModel
                {
                    Email = Email,
                    Password = Password,
                    FirstName = FirstName,
                    LastName = LastName,
                    PhoneNumber = "+216" + PhoneNumber,
                    LevelOfStudy = SelectedLevel,
                    FieldOfStudy = SelectedField,
                    ProfilePictureData = profilePictureBase64,
                    HasProfilePicture = !string.IsNullOrEmpty(profilePictureBase64)
                };

                Console.WriteLine("Calling RegisterAsync");
                var result = await _authService.RegisterAsync(registrationModel);
                Console.WriteLine($"Registration result - Success: {result.Success}, Error: {result.ErrorMessage ?? "None"}");
                
                if (result.Success)
                {
                    // Add debug logging
                    Console.WriteLine("Registration successful, navigating to Home view");
                    
                    // Clear any error message
                    ErrorMessage = string.Empty;
                    
                    try 
                    {
                        // Ensure we navigate to the Home view
                        Console.WriteLine("Attempting navigation to Home view");
                        await _navigationService.NavigateToAsync("Home");
                        Console.WriteLine("Navigation to Home completed");
                    }
                    catch (Exception navEx)
                    {
                        Console.WriteLine($"Navigation error: {navEx.Message}");
                        
                        // Try again with a different approach if the first attempt fails
                        try
                        {
                            Console.WriteLine("Attempting alternate navigation approach");
                            await Task.Delay(500); // Small delay to ensure UI is ready
                            await _navigationService.NavigateToAsync("Home");
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine($"Second navigation attempt error: {ex2.Message}");
                            ErrorMessage = "Inscription réussie, mais erreur lors de la navigation. Veuillez vous connecter manuellement.";
                        }
                    }
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Échec de l'inscription. Veuillez réessayer.";
                    Console.WriteLine($"Registration failed: {ErrorMessage}");
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Exception during registration: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
