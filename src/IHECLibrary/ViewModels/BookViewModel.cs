using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading;
using Avalonia;
using Avalonia.Platform;

namespace IHECLibrary.ViewModels
{
    public partial class BookViewModel : ViewModelBase
    {
        private readonly IBookService _bookService;
        private readonly INavigationService? _navigationService;
        private Bitmap? _coverImage;

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
        private string _availabilityColor = string.Empty;

        [ObservableProperty]
        private string _backgroundColor = string.Empty;

        [ObservableProperty]
        private string _actionButtonText = string.Empty;

        [ObservableProperty]
        private string _actionButtonBackground = string.Empty;

        [ObservableProperty]
        private bool _isLiked;
        
        [ObservableProperty]
        private string _likeButtonText = "♡";
        
        [ObservableProperty]
        private string _likeButtonColor = "#9E9E9E";
        
        [ObservableProperty]
        private decimal _rating = 0;
        
        [ObservableProperty]
        private string _ratingText = "Not rated";
        
        [ObservableProperty]
        private double _ratingPercentage = 0;

        public Bitmap? CoverImage
        {
            get => _coverImage;
            set => SetProperty(ref _coverImage, value);
        }

        public BookViewModel(BookModel book, IBookService bookService, INavigationService? navigationService = null)
        {
            _bookService = bookService;
            _navigationService = navigationService;
            
            Id = book.Id;
            Title = book.Title;
            Author = book.Author;
            Category = book.Category;
            
            // Store the image URL (which might be empty or invalid)
            CoverImageUrl = book.CoverImageUrl;
            
            // Load the image asynchronously to avoid blocking the UI
            LoadCoverImageAsync();
            
            // Set availability status and color
            bool isAvailable = book.IsAvailable();
            AvailabilityStatus = isAvailable ? "Available" : "Unavailable";
            AvailabilityColor = isAvailable ? "#4CAF50" : "#F44336"; // Green for available, red for unavailable
            
            // Set background color (optional light styling)
            BackgroundColor = "#FFFFFF"; // White background for all books
            
            // Set like status and button appearance
            IsLiked = book.IsLikedByCurrentUser;
            UpdateLikeButtonAppearance();
            
            // Set rating information
            Rating = book.RatingAverage;
            RatingPercentage = (double)(Rating / 5.0m * 100.0m);
            RatingText = Rating > 0 ? $"{Rating:0.0}/5" : "Not rated";

            // Set action button properties based on availability
            ActionButtonText = isAvailable ? "Borrow" : "View Details";
            ActionButtonBackground = isAvailable ? "#2E74A8" : "#9E9E9E"; // Blue if available, gray if not
        }

        private void UpdateLikeButtonAppearance()
        {
            LikeButtonText = IsLiked ? "♥" : "♡";
            LikeButtonColor = IsLiked ? "#F44336" : "#9E9E9E"; // Red if liked, gray if not
        }

        private async void LoadCoverImageAsync()
        {
            try
            {
                // If we have a valid cover URL from the database, try to load it
                if (!string.IsNullOrEmpty(CoverImageUrl) && Uri.IsWellFormedUriString(CoverImageUrl, UriKind.Absolute))
                {
                    using (var httpClient = new HttpClient())
                    {
                        // Set a timeout to avoid hanging if the image can't be loaded
                        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        var response = await httpClient.GetAsync(CoverImageUrl, cancellationTokenSource.Token);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var imageStream = await response.Content.ReadAsStreamAsync();
                            // Create a memory stream that we can keep open after the using block
                            var memoryStream = new MemoryStream();
                            await imageStream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;
                            
                            try
                            {
                                CoverImage = new Bitmap(memoryStream);
                                Console.WriteLine($"Successfully loaded cover for '{Title}' from URL: {CoverImageUrl}");
                                return; // Success, we're done
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error creating bitmap from stream: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Failed to download image for '{Title}': {response.StatusCode}");
                        }
                    }
                }

                // If we reach here, either the URL was invalid or the download failed
                // Load the fallback image from the Assets folder
                try
                {
                    // Use local fallback image
                    var assetPath = "avares://IHECLibrary/Assets/books.png";
                    var assets = AssetLoader.Open(new Uri(assetPath));
                    CoverImage = new Bitmap(assets);
                    Console.WriteLine($"Using fallback cover for '{Title}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not load fallback image: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading cover image: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ViewDetails()
        {
            if (_navigationService != null)
            {
                // Navigate to book details page, passing the book ID
                await _navigationService.NavigateToAsync("BookDetail", Id);
            }
        }

        [RelayCommand]
        private async Task Action()
        {
            // Determine what action to take based on the book's availability
            if (AvailabilityStatus == "Available")
            {
                // If the book is available, attempt to borrow it
                try
                {
                    bool success = await _bookService.BorrowBookAsync(Id);
                    if (success)
                    {
                        // Update UI state to reflect the borrowed status
                        AvailabilityStatus = "Unavailable";
                        AvailabilityColor = "#F44336"; // Red
                        ActionButtonText = "View Details";
                        ActionButtonBackground = "#9E9E9E"; // Gray
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error borrowing book: {ex.Message}");
                }
            }
            else
            {
                // If not available, just view the details
                await ViewDetailsCommand.ExecuteAsync(null);
            }
        }

        [RelayCommand]
        private async Task ToggleLike()
        {
            try
            {
                bool success;

                if (IsLiked)
                {
                    success = await _bookService.UnlikeBookAsync(Id);
                    if (success)
                    {
                        IsLiked = false;
                        UpdateLikeButtonAppearance();
                    }
                }
                else
                {
                    success = await _bookService.LikeBookAsync(Id);
                    if (success)
                    {
                        IsLiked = true;
                        UpdateLikeButtonAppearance();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling like: {ex.Message}");
            }
        }
    }
}