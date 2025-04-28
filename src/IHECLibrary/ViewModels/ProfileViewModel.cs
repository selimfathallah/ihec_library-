using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using IHECLibrary; // Added import for BookModel

namespace IHECLibrary.ViewModels
{
    public partial class ProfileViewModel : ViewModelBase
    {
        // User properties
        [ObservableProperty]
        private string _userFullName = string.Empty; // Remove hardcoded name

        [ObservableProperty]
        private string _userEmail = string.Empty;

        [ObservableProperty]
        private string _userPhone = string.Empty;

        [ObservableProperty]
        private string _userLevel = string.Empty;

        [ObservableProperty]
        private string _userField = string.Empty;

        [ObservableProperty]
        private string _userRank = "Bronze"; // Default rank

        [ObservableProperty]
        private string _userRankColor = "#CD7F32"; // Default bronze color

        [ObservableProperty]
        private string _userProfilePicture = "/Assets/default_profile.png"; // Default profile picture

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private int _borrowedBooksCount = 0;

        [ObservableProperty]
        private int _reservedBooksCount = 0;

        [ObservableProperty]
        private int _likedBooksCount = 0;

        [ObservableProperty]
        private double _progressValue = 25; // Default 25% progress (Bronze)

        [ObservableProperty]
        private ObservableCollection<BorrowedBookViewModel> _borrowedBooks = new();

        [ObservableProperty]
        private ObservableCollection<ReservedBookViewModel> _reservedBooks = new();

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _hasError = false;

        [ObservableProperty] 
        private string _errorMessage = string.Empty;

        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;
        private readonly IBookService _bookService;
        private readonly IAuthService _authService;

        public ProfileViewModel(INavigationService navigationService, IUserService userService, IBookService bookService, IAuthService authService)
        {
            _navigationService = navigationService;
            _userService = userService;
            _bookService = bookService;
            _authService = authService;

            // Load user data immediately
            LoadUserDataAsync();
        }

        private async void LoadUserDataAsync()
        {
            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = string.Empty;

                Console.WriteLine("ProfileViewModel: Loading user data...");
                
                // Check if user is authenticated
                bool isAuthenticated = _authService.IsAuthenticated();
                Console.WriteLine($"ProfileViewModel: Authentication status: {isAuthenticated}");
                
                if (!isAuthenticated)
                {
                    // Only show error if user is not authenticated
                    HasError = true;
                    ErrorMessage = "Please sign in to view your profile.";
                    Console.WriteLine("ProfileViewModel: User not authenticated - showing error message");
                    return;
                }
                
                // Try to get user data from the user service
                var user = await _userService.GetCurrentUserAsync();
                
                if (user == null)
                {
                    // Show error if user data is null
                    HasError = true;
                    ErrorMessage = "Unable to load your profile data. Please try logging in again.";
                    Console.WriteLine("ProfileViewModel: User is authenticated but user data is null. Showing error message.");
                    return;
                }

                // User data retrieved successfully
                Console.WriteLine($"ProfileViewModel: User data loaded: '{user.FirstName}' '{user.LastName}'");
                
                // Set basic user info - make sure to handle null or empty values
                // Set full name from first and last name or use a default
                if (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
                {
                    UserFullName = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim();
                }
                else 
                {
                    // If both first and last name are empty/null, use email or a default
                    UserFullName = !string.IsNullOrWhiteSpace(user.Email) ? 
                        user.Email : 
                        "Guest User";
                }
                
                UserEmail = user.Email ?? "";
                UserPhone = user.PhoneNumber ?? "";
                UserLevel = user.LevelOfStudy ?? "N/A";
                UserField = user.FieldOfStudy ?? "N/A";
                UserProfilePicture = !string.IsNullOrEmpty(user.ProfilePictureUrl)
                    ? user.ProfilePictureUrl
                    : "/Assets/default_profile.png";

                // Load user statistics
                try
                {
                    var statistics = await _userService.GetUserStatisticsAsync(user.Id);
                    BorrowedBooksCount = statistics.BorrowedBooksCount;
                    ReservedBooksCount = statistics.ReservedBooksCount;
                    LikedBooksCount = statistics.LikedBooksCount;
                    UserRank = statistics.Ranking;
                    
                    // Set rank color based on the user's ranking
                    UserRankColor = UserRank switch
                    {
                        "Bronze" => "#CD7F32",
                        "Silver" => "#C0C0C0", 
                        "Gold" => "#FFD700",
                        "Master" => "#9932CC",
                        _ => "#000000" 
                    };

                    // Calculate progress value for the progress bar
                    ProgressValue = UserRank switch
                    {
                        "Bronze" => 25,
                        "Silver" => 50,
                        "Gold" => 75,
                        "Master" => 100,
                        _ => 0
                    };

                    // Load borrowed books
                    BorrowedBooks.Clear();
                    foreach (var book in statistics.BorrowedBooks)
                    {
                        BorrowedBooks.Add(new BorrowedBookViewModel(book, _bookService, this));
                    }
                    Console.WriteLine($"ProfileViewModel: Loaded {BorrowedBooks.Count} borrowed books");

                    // Load reserved books
                    ReservedBooks.Clear();
                    foreach (var book in statistics.ReservedBooks)
                    {
                        ReservedBooks.Add(new ReservedBookViewModel(book, _bookService, this));
                    }
                    Console.WriteLine($"ProfileViewModel: Loaded {ReservedBooks.Count} reserved books");
                }
                catch (Exception statsEx)
                {
                    // Log but don't fail completely if statistics can't be loaded
                    Console.WriteLine($"ProfileViewModel: Error loading statistics: {statsEx.Message}");
                }
            }
            catch (Exception ex)
            {
                // Log the exception and show it to the user
                Console.WriteLine($"ProfileViewModel: Error loading profile data: {ex.Message}");
                HasError = true;
                ErrorMessage = $"Error loading profile data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Refresh the profile data
        [RelayCommand]
        private void RefreshProfile()
        {
            LoadUserDataAsync();
        }
        
        // Navigation commands remain unchanged
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
        private Task NavigateToProfile()
        {
            // We're already on the profile page, just refresh the data
            LoadUserDataAsync();
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task EditProfile()
        {
            await _navigationService.NavigateToAsync("EditProfile");
        }

        [RelayCommand]
        private async Task SignOut()
        {
            try
            {
                IsLoading = true;
                Console.WriteLine("Signing out...");
                var result = await _authService.SignOutAsync();
                
                if (result)
                {
                    Console.WriteLine("Sign out successful, navigating to Login view");
                    await _navigationService.NavigateToAsync("Login");
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Failed to sign out. Please try again.";
                    Console.WriteLine("Sign out failed");
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Error during sign out: {ex.Message}";
                Console.WriteLine($"Error signing out: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task Search()
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                try // Add try-catch for navigation
                {
                    await _navigationService.NavigateToAsync("Library", new LibraryFilterOptions { SearchQuery = SearchQuery });
                    SearchQuery = string.Empty; // Clear only on successful navigation
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error navigating to Library search: {ex.Message}");
                }
            }
        }

        // Methods for updating the UI
        internal void RemoveBorrowedBook(string bookId)
        {
            for (int i = 0; i < BorrowedBooks.Count; i++)
            {
                if (BorrowedBooks[i].Id == bookId)
                {
                    BorrowedBooks.RemoveAt(i);
                    BorrowedBooksCount--;
                    break;
                }
            }
        }

        internal void RemoveReservedBook(string bookId)
        {
            for (int i = 0; i < ReservedBooks.Count; i++)
            {
                if (ReservedBooks[i].Id == bookId)
                {
                    ReservedBooks.RemoveAt(i);
                    ReservedBooksCount--;
                    break;
                }
            }
        }
    }

    public partial class BorrowedBookViewModel : ViewModelBase
    {
        public string Id { get; }
        public string Title { get; }
        public string Author { get; }
        public string DueDate { get; }

        [ObservableProperty]
        private bool _isReturning = false;

        [ObservableProperty]
        private bool _hasError = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        private readonly IBookService _bookService;
        private readonly ProfileViewModel _parentViewModel;

        public BorrowedBookViewModel(BookModel book, IBookService bookService, ProfileViewModel parentViewModel)
        {
            _bookService = bookService;
            _parentViewModel = parentViewModel;
            Id = book.Id;
            Title = book.Title;
            Author = book.Author;

            // TODO: Replace with actual due date from the database once available
            var dueDate = DateTime.Now.AddDays(7); // Simulation
            DueDate = $"Due: {dueDate:dd/MM/yyyy}";
        }

        [RelayCommand]
        private async Task Return()
        {
            try
            {
                IsReturning = true;
                HasError = false;
                
                Console.WriteLine($"Returning book: {Id} - {Title}");
                await _bookService.ReturnBookAsync(Id);
                
                // Remove this book from the parent's collection
                _parentViewModel.RemoveBorrowedBook(Id);
                
                Console.WriteLine($"Book returned successfully: {Id}");
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Failed to return book: {ex.Message}";
                Console.WriteLine($"Error returning book {Id}: {ex.Message}");
            }
            finally
            {
                IsReturning = false;
            }
        }
    }

    public partial class ReservedBookViewModel : ViewModelBase
    {
        public string Id { get; }
        public string Title { get; }
        public string Author { get; }
        public string ReservationStatus { get; }

        [ObservableProperty]
        private bool _isCancelling = false;

        [ObservableProperty]
        private bool _hasError = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        private readonly IBookService _bookService;
        private readonly ProfileViewModel _parentViewModel;

        public ReservedBookViewModel(BookModel book, IBookService bookService, ProfileViewModel parentViewModel)
        {
            _bookService = bookService;
            _parentViewModel = parentViewModel;
            Id = book.Id;
            Title = book.Title;
            Author = book.Author;

            // Set reservation status based on book availability
            ReservationStatus = book.AvailableCopies > 0 ? "Available now" : "Waiting for availability";
        }

        [RelayCommand]
        private async Task Cancel()
        {
            try
            {
                IsCancelling = true;
                HasError = false;
                
                Console.WriteLine($"Cancelling reservation for book: {Id} - {Title}");
                await _bookService.CancelReservationAsync(Id);
                
                // Remove this book from the parent's collection
                _parentViewModel.RemoveReservedBook(Id);
                
                Console.WriteLine($"Reservation cancelled successfully: {Id}");
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Failed to cancel reservation: {ex.Message}";
                Console.WriteLine($"Error cancelling reservation for book {Id}: {ex.Message}");
            }
            finally
            {
                IsCancelling = false;
            }
        }
    }
}
