using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using IHECLibrary; // Corrected namespace for BookModel

namespace IHECLibrary.ViewModels
{
    public partial class HomeViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _welcomeMessage = string.Empty;

        [ObservableProperty]
        private string _recommendationSubtitle = string.Empty;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private string _userFullName = string.Empty;

        [ObservableProperty]
        private string _userProfilePicture = string.Empty;

        [ObservableProperty]
        private ObservableCollection<BookViewModel> _recommendedBooks = new();

        private readonly INavigationService _navigationService;
        private readonly IBookService _bookService;
        private readonly IUserService _userService;

        public HomeViewModel(INavigationService navigationService, IBookService bookService, IUserService userService)
        {
            _navigationService = navigationService;
            _bookService = bookService;
            _userService = userService;

            LoadUserData();
            LoadRecommendedBooks();
        }

        private async void LoadUserData()
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user != null)
            {
                UserFullName = $"{user.FirstName} {user.LastName}";
                UserProfilePicture = user.ProfilePictureUrl ?? "/Assets/default_profile.png";
                WelcomeMessage = $"Welcome back, {user.FirstName}!";
                RecommendationSubtitle = $"Here are some recommendations for your {user.FieldOfStudy} studies";
            }
        }

        private async void LoadRecommendedBooks()
        {
            var books = await _bookService.GetRecommendedBooksAsync();
            RecommendedBooks.Clear();
            foreach (var book in books)
            {
                RecommendedBooks.Add(new BookViewModel(book, _bookService));
            }
        }

        [RelayCommand]
        private Task NavigateToHome()
        {
            // Déjà sur la page d'accueil, ne rien faire
            return Task.CompletedTask;
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

        [RelayCommand]
        private async Task ViewAllRecommendations()
        {
            await _navigationService.NavigateToAsync("Library", new LibraryFilterOptions { Category = "Recommended" });
        }

        [RelayCommand]
        private async Task Search()
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                await _navigationService.NavigateToAsync("Library", new LibraryFilterOptions { SearchQuery = SearchQuery });
                SearchQuery = string.Empty;
            }
        }
    }
}
