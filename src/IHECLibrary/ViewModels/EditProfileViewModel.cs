using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;

namespace IHECLibrary.ViewModels
{
    public partial class EditProfileViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;
        
        [ObservableProperty]
        private string firstName = string.Empty;
        
        [ObservableProperty]
        private string lastName = string.Empty;
        
        [ObservableProperty]
        private string phoneNumber = string.Empty;
        
        [ObservableProperty]
        private string levelOfStudy = string.Empty;
        
        [ObservableProperty]
        private string fieldOfStudy = string.Empty;
        
        [ObservableProperty]
        private string profilePictureUrl = string.Empty;
        
        [ObservableProperty]
        private Bitmap? profileImage;

        [ObservableProperty]
        private string selectedImagePath = string.Empty;
        
        [ObservableProperty]
        private string errorMessage = string.Empty;
        
        // Add property to check if there is an error message
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        
        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string ranking = string.Empty;
        
        [ObservableProperty]
        private int booksBorrowed;
        
        [ObservableProperty]
        private int booksReserved;
        
        public EditProfileViewModel(IUserService userService, INavigationService navigationService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            
            LoadProfileData();
        }
        
        private async void LoadProfileData()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            try
            {
                var userProfile = await _userService.GetCurrentUserProfileAsync();
                if (userProfile != null)
                {
                    FirstName = userProfile.FirstName ?? string.Empty;
                    LastName = userProfile.LastName ?? string.Empty;
                    PhoneNumber = userProfile.PhoneNumber ?? string.Empty;
                    LevelOfStudy = userProfile.LevelOfStudy ?? string.Empty;
                    FieldOfStudy = userProfile.FieldOfStudy ?? string.Empty;
                    ProfilePictureUrl = userProfile.ProfilePictureUrl ?? string.Empty;
                    Ranking = userProfile.Ranking ?? string.Empty;
                    BooksBorrowed = userProfile.BooksBorrowed;
                    BooksReserved = userProfile.BooksReserved;
                    
                    if (!string.IsNullOrEmpty(ProfilePictureUrl))
                    {
                        try
                        {
                            // Load profile image if URL is available
                            if (File.Exists(ProfilePictureUrl))
                            {
                                using (var stream = File.OpenRead(ProfilePictureUrl))
                                {
                                    ProfileImage = new Bitmap(stream);
                                }
                            }
                            // If it's a web URL, you'd need to implement image loading from web
                        }
                        catch (Exception ex)
                        {
                            // Handle image loading failure silently
                            System.Diagnostics.Debug.WriteLine($"Failed to load profile image: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading profile: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        [RelayCommand]
        private async Task SelectProfilePicture()
        {
            try
            {
                // Get the top-level window for the storage provider
                var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime 
                    ? lifetime.MainWindow : null;
                if (mainWindow == null)
                    return;

                var storageProvider = mainWindow.StorageProvider;
                var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Profile Picture",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Image Files")
                        {
                            Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp" }
                        }
                    }
                });

                if (files != null && files.Count > 0)
                {
                    var file = files[0];
                    SelectedImagePath = file.Path.LocalPath;
                    
                    // Load the selected image
                    using (var stream = await file.OpenReadAsync())
                    {
                        ProfileImage = new Bitmap(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error selecting image: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task SaveProfile()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            try
            {
                // Create update model
                var updateModel = new UserProfileUpdateModel
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    PhoneNumber = PhoneNumber,
                    LevelOfStudy = LevelOfStudy,
                    FieldOfStudy = FieldOfStudy
                };

                // Handle profile picture if a new one was selected
                if (!string.IsNullOrEmpty(SelectedImagePath))
                {
                    // In a real app, you would upload this file to storage and get a URL
                    // For this example, we'll just use the local path
                    updateModel.ProfilePictureUrl = SelectedImagePath;
                }
                
                System.Diagnostics.Debug.WriteLine("Saving profile changes to database...");
                Console.WriteLine($"Updating profile: {FirstName} {LastName}, Phone: {PhoneNumber}, Level: {LevelOfStudy}, Field: {FieldOfStudy}");
                
                // Update profile
                var result = await _userService.UpdateUserProfileAsync(updateModel);
                
                if (result)
                {
                    // Log this activity
                    LogProfileUpdateActivity();
                    
                    System.Diagnostics.Debug.WriteLine("Profile updated successfully, attempting to navigate back to profile page");
                    
                    try
                    {
                        // Force navigation directly to profile page
                        await _navigationService.NavigateToAsync("Profile");
                        System.Diagnostics.Debug.WriteLine("Navigation to Profile page completed successfully");
                    }
                    catch (Exception navEx)
                    {
                        // Log navigation error but don't show to user
                        System.Diagnostics.Debug.WriteLine($"Navigation error: {navEx.Message}");
                        Console.WriteLine($"Navigation error: {navEx.Message}");
                        
                        // Try again with a different approach
                        try 
                        {
                            await Task.Delay(100); // Small delay
                            await _navigationService.NavigateToAsync("Profile");
                            System.Diagnostics.Debug.WriteLine("Second navigation attempt succeeded");
                        }
                        catch (Exception secondEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Second navigation attempt failed: {secondEx.Message}");
                            // We don't want to set ErrorMessage here as the profile update was successful
                        }
                    }
                }
                else
                {
                    ErrorMessage = "Failed to update profile. Please try again.";
                    System.Diagnostics.Debug.WriteLine("Failed to update profile in database");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving profile: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Exception when saving profile: {ex.Message}");
                Console.WriteLine($"Exception when saving profile: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private static void LogProfileUpdateActivity()
        {
            try 
            {
                // Assuming there's an activity logging service
                // This would typically be injected, but for brevity we're not adding it
                // This is a placeholder for where you would log the activity
                System.Diagnostics.Debug.WriteLine("Profile updated successfully - activity logged");
            }
            catch (Exception ex)
            {
                // Silently handle logging errors
                System.Diagnostics.Debug.WriteLine($"Failed to log activity: {ex.Message}");
            }
        }
        
        [RelayCommand]
        private async Task Cancel()
        {
            await _navigationService.NavigateToAsync("Profile");
        }
    }
}