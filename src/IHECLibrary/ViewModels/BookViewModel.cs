using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IHECLibrary.ViewModels
{
    public partial class BookViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _id = string.Empty;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _author = string.Empty;

        [ObservableProperty]
        private string _category = string.Empty;

        [ObservableProperty]
        private string _coverImageUrl = string.Empty;

        [ObservableProperty]
        private bool _isAvailable;

        [ObservableProperty]
        private string _availabilityStatus = string.Empty;

        [ObservableProperty]
        private string _availabilityColor = "#4CAF50"; // Default green color for Available

        [ObservableProperty]
        private string _backgroundColor = "#FFFFFF"; // Default background color
        
        [ObservableProperty]
        private Color _cardColor;

        [ObservableProperty]
        private bool _isLiked = false;

        [ObservableProperty]
        private string _actionButtonText = "View Details";

        [ObservableProperty]
        private string _actionButtonBackground = "#2E74A8"; // Default blue color for action button

        // Common card background colors to rotate through
        private static readonly Color[] CardColors = new[]
        {
            Color.Parse("#e3f2fd"), // Light Blue
            Color.Parse("#f3e5f5"), // Light Purple
            Color.Parse("#e8f5e9"), // Light Green 
            Color.Parse("#fff3e0"), // Light Orange
            Color.Parse("#ffebee")  // Light Red
        };
        
        // Static counter to rotate through colors for consecutive books
        private static int colorIndex = 0;
        
        // Static constructor to initialize the color index with a more random starting point
        static BookViewModel()
        {
            colorIndex = DateTime.Now.Second % CardColors.Length;
        }

        public BookViewModel(BookModel book, IBookService bookService, INavigationService? navigationService = null)
        {
            _bookService = bookService;
            _navigationService = navigationService;
            
            Id = book.Id;
            Title = book.Title;
            Author = book.Author;
            Category = book.Category;
            
            // Improved placeholder image handling - more resilient
            if (!string.IsNullOrEmpty(book.CoverImageUrl) && Uri.IsWellFormedUriString(book.CoverImageUrl, UriKind.Absolute))
            {
                CoverImageUrl = book.CoverImageUrl;
            }
            else
            {
                // Use a more reliable placeholder service with book title encoded properly
                string encodedTitle = Uri.EscapeDataString(book.Title?.Length > 20 ? book.Title.Substring(0, 20) : (book.Title ?? "Book"));
                CoverImageUrl = $"https://placehold.co/200x300/e8e8e8/4a4a4a?text={encodedTitle}";
            }
            
            // Set availability status and properties
            IsAvailable = book.IsAvailable();
            AvailabilityStatus = IsAvailable ? "Available" : "Unavailable";
            AvailabilityColor = IsAvailable ? "#4CAF50" : "#F44336"; // Green for available, red for unavailable
            
            // Assign a background color from our rotation - CRITICAL for UI display
            CardColor = CardColors[colorIndex];
            colorIndex = (colorIndex + 1) % CardColors.Length;
            
            IsLiked = book.IsLikedByCurrentUser;

            // Set action button properties based on availability
            ActionButtonText = IsAvailable ? "Borrow" : "Reserve";
            ActionButtonBackground = IsAvailable ? "#2E74A8" : "#9E9E9E"; 
            
            Console.WriteLine($"Created BookViewModel: {Title}, IsAvailable: {IsAvailable}, CardColor: {CardColor}");
        }

        public ICommand ActionCommand => ViewDetailsCommand;

        private readonly IBookService _bookService;
        private readonly INavigationService? _navigationService;

        [RelayCommand]
        private void ViewDetails()
        {
            try
            {
                // Navigate to book details if navigation service is available
                _navigationService?.NavigateToAsync("BookDetails", Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error viewing book details: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ToggleLike()
        {
            try
            {
                bool wasLiked = IsLiked;
                IsLiked = !wasLiked;
                
                // Call the appropriate API method based on the new state
                if (IsLiked)
                {
                    await _bookService.LikeBookAsync(Id);
                }
                else
                {
                    await _bookService.UnlikeBookAsync(Id);
                }
            }
            catch (Exception ex)
            {
                // If the API call fails, revert the UI change
                IsLiked = !IsLiked;
                Console.WriteLine($"Error toggling book like: {ex.Message}");
            }
        }
    }
}