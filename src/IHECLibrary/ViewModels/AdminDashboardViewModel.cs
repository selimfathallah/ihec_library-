using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace IHECLibrary.ViewModels
{
    public partial class AdminDashboardViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _adminFullName = string.Empty;

        [ObservableProperty]
        private string _adminProfilePicture = string.Empty;

        [ObservableProperty]
        private int _totalBooksCount = 0;

        [ObservableProperty]
        private int _totalUsersCount = 0;

        [ObservableProperty]
        private int _activeBorrowingsCount = 0;

        [ObservableProperty]
        private int _pendingReservationsCount = 0;

        [ObservableProperty]
        private ObservableCollection<ActivityViewModel> _recentActivities = new();

        [ObservableProperty]
        private ObservableCollection<PopularBookViewModel> _popularBooks = new();

        [ObservableProperty]
        private ObservableCollection<ActiveUserViewModel> _activeUsers = new();

        private readonly INavigationService _navigationService;
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;
        private readonly IBookService _bookService;
        private readonly IAuthService _authService;

        public AdminDashboardViewModel(INavigationService navigationService, IAdminService adminService, IUserService userService, IBookService bookService, IAuthService authService)
        {
            _navigationService = navigationService;
            _adminService = adminService;
            _userService = userService;
            _bookService = bookService;
            _authService = authService;

            LoadAdminData();
            LoadDashboardData();
        }

        private async void LoadAdminData()
        {
            var admin = await _adminService.GetCurrentAdminAsync();
            if (admin != null)
            {
                AdminFullName = $"{admin.FirstName} {admin.LastName}";
                AdminProfilePicture = admin.ProfilePictureUrl ?? "/Assets/default_admin_profile.png";
            }
        }

        private async void LoadDashboardData()
        {
            var dashboardData = await _adminService.GetDashboardDataAsync();
            
            TotalBooksCount = dashboardData.TotalBooksCount;
            TotalUsersCount = dashboardData.TotalUsersCount;
            ActiveBorrowingsCount = dashboardData.ActiveBorrowingsCount;
            PendingReservationsCount = dashboardData.PendingReservationsCount;

            // Charger les activités récentes
            RecentActivities.Clear();
            foreach (var activity in dashboardData.RecentActivities)
            {
                RecentActivities.Add(new ActivityViewModel
                {
                    ActivityTitle = activity.Title,
                    ActivityDescription = activity.Description,
                    ActivityTime = activity.Time,
                    ActivityIcon = GetActivityIcon(activity.Type),
                    ActivityColor = GetActivityColor(activity.Type)
                });
            }

            // Charger les livres populaires
            PopularBooks.Clear();
            foreach (var book in dashboardData.PopularBooks)
            {
                PopularBooks.Add(new PopularBookViewModel(book, _navigationService));
            }

            // Charger les utilisateurs actifs
            ActiveUsers.Clear();
            foreach (var user in dashboardData.ActiveUsers)
            {
                ActiveUsers.Add(new ActiveUserViewModel(user, _navigationService));
            }
        }

        private string GetActivityIcon(string activityType)
        {
            return activityType switch
            {
                "Borrow" => "📚",
                "Return" => "📖",
                "Register" => "👤",
                "AddBook" => "➕",
                _ => "🔔"
            };
        }

        private string GetActivityColor(string activityType)
        {
            return activityType switch
            {
                "Borrow" => "#2E74A8",
                "Return" => "#4CAF50",
                "Register" => "#9C27B0",
                "AddBook" => "#FF9800",
                _ => "#607D8B"
            };
        }

        [RelayCommand]
        private void NavigateToDashboard()
        {
            // Déjà sur le dashboard, ne rien faire
        }

        [RelayCommand]
        private async Task NavigateToBooks()
        {
            await _navigationService.NavigateToAsync("AdminBooks");
        }

        [RelayCommand]
        private async Task NavigateToUsers()
        {
            await _navigationService.NavigateToAsync("AdminUsers");
        }

        [RelayCommand]
        private async Task NavigateToBorrowings()
        {
            await _navigationService.NavigateToAsync("AdminBorrowings");
        }

        [RelayCommand]
        private async Task NavigateToReservations()
        {
            await _navigationService.NavigateToAsync("AdminReservations");
        }

        [RelayCommand]
        private async Task NavigateToAdmins()
        {
            await _navigationService.NavigateToAsync("AdminAccounts");
        }

        [RelayCommand]
        private async Task NavigateToSettings()
        {
            await _navigationService.NavigateToAsync("AdminSettings");
        }

        [RelayCommand]
        private async Task SignOut()
        {
            // Use the _authService (not _adminService) for sign out functionality
            var result = await _authService.SignOutAsync();
            if (result)
            {
                await _navigationService.NavigateToAsync("AdminLogin");
            }
        }
    }

    public partial class ActivityViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _activityTitle = string.Empty;

        [ObservableProperty]
        private string _activityDescription = string.Empty;

        [ObservableProperty]
        private string _activityTime = string.Empty;

        [ObservableProperty]
        private string _activityIcon = string.Empty;

        [ObservableProperty]
        private string _activityColor = string.Empty;
    }

    public partial class PopularBookViewModel : ViewModelBase
    {
        public string Id { get; }
        public string Title { get; }
        public string Author { get; }
        public int BorrowCount { get; }

        private readonly INavigationService _navigationService;

        public PopularBookViewModel(PopularBookModel book, INavigationService navigationService)
        {
            _navigationService = navigationService;
            Id = book.Id;
            Title = book.Title;
            Author = book.Author;
            BorrowCount = book.BorrowCount;
        }

        [RelayCommand]
        private async Task ViewDetails()
        {
            await _navigationService.NavigateToAsync("AdminBookDetails", Id);
        }
    }

    public partial class ActiveUserViewModel : ViewModelBase
    {
        public string Id { get; }
        public string FullName { get; }
        public string Email { get; }
        public string ProfilePicture { get; }
        public int BorrowedBooksCount { get; }

        private readonly INavigationService _navigationService;

        public ActiveUserViewModel(ActiveUserModel user, INavigationService navigationService)
        {
            _navigationService = navigationService;
            Id = user.Id;
            FullName = $"{user.FirstName} {user.LastName}";
            Email = user.Email;
            ProfilePicture = user.ProfilePictureUrl ?? "/Assets/default_profile.png";
            BorrowedBooksCount = user.BorrowedBooksCount;
        }

        [RelayCommand]
        private async Task ViewProfile()
        {
            await _navigationService.NavigateToAsync("AdminUserDetails", Id);
        }
    }
}
