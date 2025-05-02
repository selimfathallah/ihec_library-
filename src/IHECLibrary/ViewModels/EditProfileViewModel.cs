using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Threading.Tasks;

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

        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;

        public EditProfileViewModel(IUserService userService, INavigationService navigationService)
        {
            _userService = userService;
            _navigationService = navigationService;
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
        private async Task Save()
        {
            try
            {
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
                    NotifyNewBooks = NotifyNewBooks
                };

                var result = await _userService.UpdateUserProfileAsync(updateModel);
                if (result)
                {
                    await _navigationService.NavigateToAsync("Profile");
                }
                else
                {
                    Console.WriteLine("Failed to save profile changes.");
                }
            }
            catch (Exception ex)
            {
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