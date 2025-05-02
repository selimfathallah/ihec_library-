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
        [ObservableProperty]
        private string _userFullName = string.Empty;

        [ObservableProperty]
        private string _userEmail = string.Empty;

        [ObservableProperty]
        private string _userPhone = string.Empty;

        [ObservableProperty]
        private string _userLevel = string.Empty;

        [ObservableProperty]
        private string _userField = string.Empty;

        [ObservableProperty]
        private string _userRank = string.Empty;

        [ObservableProperty]
        private string _userRankColor = string.Empty;

        [ObservableProperty]
        private string _userProfilePicture = string.Empty;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private int _borrowedBooksCount = 0;

        [ObservableProperty]
        private int _reservedBooksCount = 0;

        [ObservableProperty]
        private int _likedBooksCount = 0;

        [ObservableProperty]
        private double _progressValue = 0;

        [ObservableProperty]
        private ObservableCollection<BorrowedBookViewModel> _borrowedBooks = new();

        [ObservableProperty]
        private ObservableCollection<ReservedBookViewModel> _reservedBooks = new();

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

            // Use Task.Run for async void constructor pattern or implement an async init method
            // For simplicity here, keeping async void but adding try-catch
            LoadUserData();
        }

        private async void LoadUserData()
        {
            try // Add try-catch for error handling
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user != null)
                {
                    UserFullName = $"{user.FirstName} {user.LastName}";
                    UserEmail = user.Email;
                    UserPhone = user.PhoneNumber;
                    UserLevel = user.LevelOfStudy ?? "N/A";
                    UserField = user.FieldOfStudy ?? "N/A";
                    UserProfilePicture = user.ProfilePictureUrl ?? "/Assets/default_profile.png";

                    var statistics = await _userService.GetUserStatisticsAsync(user.Id);
                    BorrowedBooksCount = statistics.BorrowedBooksCount;
                    ReservedBooksCount = statistics.ReservedBooksCount;
                    LikedBooksCount = statistics.LikedBooksCount;
                    UserRank = statistics.Ranking;

                    // Définir la couleur du rang
                    UserRankColor = UserRank switch
                    {
                        "Bronze" => "#CD7F32",
                        "Silver" => "#C0C0C0",
                        "Gold" => "#FFD700",
                        "Master" => "#9932CC",
                        _ => "#000000"
                    };

                    // Calculer la valeur de progression
                    ProgressValue = UserRank switch
                    {
                        "Bronze" => 25,
                        "Silver" => 50,
                        "Gold" => 75,
                        "Master" => 100,
                        _ => 0
                    };

                    // Charger les livres empruntés
                    BorrowedBooks.Clear();
                    foreach (var book in statistics.BorrowedBooks)
                    {
                        // TODO: Ensure BorrowedBookViewModel uses actual due date from book/statistics data
                        BorrowedBooks.Add(new BorrowedBookViewModel(book, _bookService));
                    }

                    // Charger les livres réservés
                    ReservedBooks.Clear();
                    foreach (var book in statistics.ReservedBooks)
                    {
                        // TODO: Ensure ReservedBookViewModel uses actual reservation status from book/statistics data
                        ReservedBooks.Add(new ReservedBookViewModel(book, _bookService));
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error or display a message to the user
                Console.WriteLine($"Error loading user data: {ex.Message}");
                // Optionally, display an error message in the UI
            }
        }

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
            // Déjà sur la page de profil, ne rien faire
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task EditProfile()
        {
            try
            {
                await _navigationService.NavigateToAsync("EditProfile");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to Edit Profile page: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SignOut()
        {
            try // Add try-catch for sign out
            {
                var result = await _authService.SignOutAsync();
                if (result)
                {
                    await _navigationService.NavigateToAsync("Login");
                }
                // Optionally handle the case where sign out fails
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error signing out: {ex.Message}");
                // Optionally, display an error message in the UI
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
                    // Optionally, display an error message to the user
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

        private readonly IBookService _bookService;

        public BorrowedBookViewModel(BookModel book, IBookService bookService)
        {
            _bookService = bookService;
            Id = book.Id;
            Title = book.Title;
            Author = book.Author;

            // TODO: Replace simulated due date with actual data
            // Example: DueDate = $"Due: {book.DueDateProperty:dd/MM/yyyy}"; // Assuming BookModel has a DueDateProperty
            var dueDate = DateTime.Now.AddDays(7); // Simulation
            DueDate = $"Due: {dueDate:dd/MM/yyyy}";
        }

        [RelayCommand]
        private async Task Return()
        {
            try // Add try-catch
            {
                await _bookService.ReturnBookAsync(Id);
                // Optionally: Add logic to remove this item from the parent collection upon success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error returning book {Id}: {ex.Message}");
                // Optionally, display an error message in the UI
            }
        }
    }

    public partial class ReservedBookViewModel : ViewModelBase
    {
        public string Id { get; }
        public string Title { get; }
        public string Author { get; }
        public string ReservationStatus { get; }

        private readonly IBookService _bookService;

        public ReservedBookViewModel(BookModel book, IBookService bookService)
        {
            _bookService = bookService;
            Id = book.Id;
            Title = book.Title;
            Author = book.Author;

            // TODO: Replace simulated status with actual data
            // Example: ReservationStatus = book.IsAvailable ? "Available now" : "Waiting for availability"; // Assuming BookModel has relevant property
            ReservationStatus = book.AvailableCopies > 0 ? "Available now" : "Waiting for availability"; // Simulation based on AvailableCopies
        }

        [RelayCommand]
        private async Task Cancel()
        {
            try // Add try-catch
            {
                // Annuler la réservation (Implement using IBookService)
                // Assuming a method like CancelReservationAsync exists in IBookService
                await _bookService.CancelReservationAsync(Id);
                // Optionally: Add logic to remove this item from the parent collection upon success
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"Error cancelling reservation for book {Id}: {ex.Message}");
                 // Optionally, display an error message in the UI
            }
        }
    }
}
