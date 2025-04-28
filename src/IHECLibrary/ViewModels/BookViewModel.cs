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
        private string _availabilityStatus = string.Empty;

        [ObservableProperty]
        private string _availabilityColor = "#4CAF50"; // Default green color for Available

        [ObservableProperty]
        private string _backgroundColor = "#FFFFFF"; // Default background color

        [ObservableProperty]
        private bool _isLiked = false;

        [ObservableProperty]
        private string _actionButtonText = "View Details";

        [ObservableProperty]
        private string _actionButtonBackground = "#2E74A8"; // Default blue color for action button

        public ICommand ActionCommand => ViewDetailsCommand;

        private readonly IBookService _bookService;
        private readonly INavigationService? _navigationService;

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
            
            // Set availability status and color
            bool isAvailable = book.IsAvailable();
            AvailabilityStatus = isAvailable ? "Available" : "Unavailable";
            AvailabilityColor = isAvailable ? "#4CAF50" : "#F44336"; // Green for available, red for unavailable
            
            // Set background color (optional light styling)
            BackgroundColor = "#FFFFFF"; // White background for all books
            
            IsLiked = book.IsLikedByCurrentUser;

            // Set action button properties based on availability
            ActionButtonText = isAvailable ? "Borrow" : "View Details";
            ActionButtonBackground = isAvailable ? "#2E74A8" : "#9E9E9E"; // Blue if available, gray if not
        }

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