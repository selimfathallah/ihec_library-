using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Threading.Tasks;
using System.IO;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;

namespace IHECLibrary.ViewModels
{
    public partial class EditProfileViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _phoneNumber = string.Empty;

        [ObservableProperty]
        private string _levelOfStudy = string.Empty;

        [ObservableProperty]
        private string _fieldOfStudy = string.Empty;

        [ObservableProperty]
        private string _studentId = string.Empty;

        [ObservableProperty]
        private string _department = string.Empty;

        [ObservableProperty]
        private string _preferences = string.Empty;

        [ObservableProperty]
        private bool _notifyReturns = true;

        [ObservableProperty]
        private bool _notifyReservations = true;

        [ObservableProperty]
        private bool _notifyNewBooks = true;

        [ObservableProperty]
        private string _profilePicturePath = string.Empty;

        [ObservableProperty]
        private string _currentProfilePictureUrl = string.Empty;

        [ObservableProperty]
        private Bitmap? _profilePicturePreview;

        [ObservableProperty]
        private bool _hasNewProfilePicture;

        [ObservableProperty]
        private string _profilePictureDisplay = "Change Profile Picture";

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;
        private readonly Window? _parentWindow;

        public EditProfileViewModel(IUserService userService, INavigationService navigationService)
        {
            _userService = userService;
            _navigationService = navigationService;
            
            // Get parent window for file picker
            _parentWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
                
            LoadUserData();
        }

        private async void LoadUserData()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user != null)
                {
                    FirstName = user.FirstName;
                    LastName = user.LastName;
                    Email = user.Email;
                    PhoneNumber = user.PhoneNumber ?? string.Empty;
                    LevelOfStudy = user.LevelOfStudy ?? string.Empty;
                    FieldOfStudy = user.FieldOfStudy ?? string.Empty;
                    StudentId = user.StudentId ?? string.Empty;
                    Department = user.Department ?? string.Empty;
                    Preferences = user.Preferences ?? string.Empty;
                    CurrentProfilePictureUrl = user.ProfilePictureUrl ?? string.Empty;

                    // Load notification preferences if available
                    NotifyReturns = user.NotifyReturns;
                    NotifyReservations = user.NotifyReservations;
                    NotifyNewBooks = user.NotifyNewBooks;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user data: {ex.Message}");
            }
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
                    HasNewProfilePicture = true;

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
        private async Task Save()
        {
            try
            {
                // Prepare profile picture data if a new one was selected
                string profilePictureBase64 = string.Empty;
                if (HasNewProfilePicture && !string.IsNullOrEmpty(ProfilePicturePath))
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
                
                var updateModel = new UserProfileUpdateModel
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    PhoneNumber = PhoneNumber,
                    LevelOfStudy = LevelOfStudy,
                    FieldOfStudy = FieldOfStudy,
                    Preferences = Preferences,
                    NotifyReturns = NotifyReturns,
                    NotifyReservations = NotifyReservations,
                    NotifyNewBooks = NotifyNewBooks,
                    HasNewProfilePicture = HasNewProfilePicture,
                    ProfilePictureData = profilePictureBase64
                };

                var result = await _userService.UpdateUserProfileAsync(updateModel);
                if (result)
                {
                    await _navigationService.NavigateToAsync("Profile");
                }
                else
                {
                    ErrorMessage = "Failed to save profile changes.";
                    Console.WriteLine("Failed to save profile changes.");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                Console.WriteLine($"Error saving profile changes: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await _navigationService.NavigateToAsync("Profile");
        }

        // Navigation commands
        [RelayCommand]
        private async Task NavigateToHome()
        {
            await _navigationService.NavigateToAsync("Home");
        }

        [RelayCommand]
        private async Task NavigateToLibrary()
        {
            await _navigationService.NavigateToAsync("Library");
        }

        [RelayCommand]
        private async Task NavigateToChatbot()
        {
            await _navigationService.NavigateToAsync("Chatbot");
        }

        [RelayCommand]
        private async Task NavigateToProfile()
        {
            await _navigationService.NavigateToAsync("Profile");
        }
    }
}