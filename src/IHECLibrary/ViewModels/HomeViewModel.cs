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

    public partial class BookViewModel : ViewModelBase
    {
        public string Id { get; }
        public string Title { get; }
        public string Author { get; }
        public string Category { get; }
        public string BackgroundColor { get; }
        public string AvailabilityStatus { get; }
        public string AvailabilityColor { get; }
        public string ActionButtonText { get; }
        public string ActionButtonBackground { get; }

        private readonly IBookService _bookService;

        public BookViewModel(BookModel book, IBookService bookService)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            Id = book.Id;
            Title = book.Title;
            Author = book.Author;
            Category = $"Category: {book.Category}";

            // Couleurs de fond aléatoires pour les cartes de livres
            string[] colors = { "#E6F2F8", "#E6F0FF", "#E6FFE6", "#FFF0E6" };
            int colorIndex = book.Id.GetHashCode() % colors.Length;
            BackgroundColor = colors[Math.Abs(colorIndex)];

            // Statut de disponibilité
            if (book.AvailableCopies > 0)
            {
                AvailabilityStatus = "Available";
                AvailabilityColor = "#4CAF50"; // Vert
                ActionButtonText = "Borrow";
                ActionButtonBackground = "#2E74A8"; // Bleu
            }
            else
            {
                AvailabilityStatus = "Unavailable";
                AvailabilityColor = "#F44336"; // Rouge
                ActionButtonText = "Reserve";
                ActionButtonBackground = "#F44336"; // Rouge
            }
        }

        [RelayCommand]
        private async Task Action()
        {
            if (AvailabilityStatus == "Available")
            {
                await _bookService.BorrowBookAsync(Id);
            }
            else
            {
                await _bookService.ReserveBookAsync(Id);
            }
        }
    }

    public class LibraryFilterOptions
    {
        public string SearchQuery { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
