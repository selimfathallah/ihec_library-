using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using IHECLibrary; // Added import for BookModel

namespace IHECLibrary.ViewModels
{
    public partial class LibraryViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _pageTitle = "All Books";

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private string _userFullName = string.Empty;

        [ObservableProperty]
        private string _userProfilePicture = string.Empty;

        [ObservableProperty]
        private ObservableCollection<BookViewModel> _books = new();

        [ObservableProperty]
        private ObservableCollection<CategoryViewModel> _categories = new();

        [ObservableProperty]
        private ObservableCollection<LanguageViewModel> _languages = new();

        [ObservableProperty]
        private bool _isAvailableOnly = false;

        [ObservableProperty]
        private string _selectedSortOption = string.Empty;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _hasNextPage = false;

        [ObservableProperty]
        private bool _hasPreviousPage = false;

        private const int PAGE_SIZE = 12;

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Most Popular", "Newest", "Title A-Z", "Author A-Z"
        };

        private readonly INavigationService _navigationService;
        private readonly IBookService _bookService;
        private readonly IUserService _userService;
        private LibraryFilterOptions? _initialFilters;

        public LibraryViewModel(INavigationService navigationService, IBookService bookService, IUserService userService, object? parameter = null)
        {
            _navigationService = navigationService;
            _bookService = bookService;
            _userService = userService;
            _initialFilters = parameter as LibraryFilterOptions;

            SelectedSortOption = SortOptions[0];
            InitializeCategories();
            InitializeLanguages();
            LoadUserData();
            LoadBooks();
        }

        private void InitializeCategories()
        {
            Categories.Add(new CategoryViewModel { Name = "Finance", IsSelected = false });
            Categories.Add(new CategoryViewModel { Name = "Management", IsSelected = false });
            Categories.Add(new CategoryViewModel { Name = "Marketing", IsSelected = false });
            Categories.Add(new CategoryViewModel { Name = "Economics", IsSelected = false });
            Categories.Add(new CategoryViewModel { Name = "Accounting", IsSelected = false });
            Categories.Add(new CategoryViewModel { Name = "BI", IsSelected = true });
            Categories.Add(new CategoryViewModel { Name = "Big Data", IsSelected = false });

            // Si des filtres initiaux sont fournis, les appliquer
            if (_initialFilters != null && !string.IsNullOrEmpty(_initialFilters.Category))
            {
                foreach (var category in Categories)
                {
                    category.IsSelected = category.Name == _initialFilters.Category;
                }
                PageTitle = $"{_initialFilters.Category} Books";
            }
        }

        private void InitializeLanguages()
        {
            Languages.Add(new LanguageViewModel { Name = "English", IsSelected = true });
            Languages.Add(new LanguageViewModel { Name = "French", IsSelected = false });
            Languages.Add(new LanguageViewModel { Name = "Arabic", IsSelected = false });
        }

        private async void LoadUserData()
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user != null)
            {
                UserFullName = $"{user.FirstName} {user.LastName}";
                UserProfilePicture = user.ProfilePictureUrl ?? "/Assets/default_profile.png";
            }
        }

        private async void LoadBooks()
        {
            IsLoading = true;
            try
            {
                List<BookModel> books;
                string? categoryFilter = null;
                string? searchFilter = null;

                // Si des filtres initiaux sont fournis, les appliquer
                if (_initialFilters != null && !string.IsNullOrEmpty(_initialFilters.SearchQuery))
                {
                    SearchQuery = _initialFilters.SearchQuery;
                    searchFilter = SearchQuery;
                    PageTitle = $"Search Results: {SearchQuery}";
                }
                else if (_initialFilters != null && !string.IsNullOrEmpty(_initialFilters.Category))
                {
                    categoryFilter = _initialFilters.Category;
                }
                else
                {
                    // Get selected categories for the filter
                    var selectedCategories = Categories.Where(c => c.IsSelected).Select(c => c.Name).ToList();
                    if (selectedCategories.Count > 0)
                    {
                        categoryFilter = selectedCategories.First(); // Use first category for the filter
                    }
                }

                // Use the new GetRealBooksAsync method to fetch books with pagination
                books = await _bookService.GetRealBooksAsync(
                    page: CurrentPage,
                    pageSize: PAGE_SIZE,
                    category: categoryFilter,
                    searchQuery: searchFilter
                );

                // Apply additional filtering if needed
                if (IsAvailableOnly)
                {
                    books = books.Where(b => b.AvailableCopies > 0).ToList();
                }

                // Apply language filtering
                var selectedLanguage = Languages.FirstOrDefault(l => l.IsSelected)?.Name;
                if (!string.IsNullOrEmpty(selectedLanguage))
                {
                    books = books.Where(b => 
                        string.IsNullOrEmpty(b.Language) || 
                        b.Language.Equals(selectedLanguage, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                // Calculate pagination info
                // Estimate total pages based on current results
                // In a real world scenario, we'd want to get an actual count from the API
                TotalPages = Math.Max(1, (int)Math.Ceiling(books.Count / (double)PAGE_SIZE));
                HasNextPage = CurrentPage < TotalPages;
                HasPreviousPage = CurrentPage > 1;

                // Apply sorting
                books = SortBooks(books);

                // Update the book collection
                Books.Clear();
                foreach (var book in books)
                {
                    Books.Add(new BookViewModel(book, _bookService));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading books: {ex.Message}");
                // You might want to add error handling UI here
            }
            finally
            {
                IsLoading = false;
            }
        }

        private List<BookModel> SortBooks(List<BookModel> books)
        {
            return SelectedSortOption switch
            {
                "Most Popular" => books.OrderByDescending(b => b.LikesCount).ToList(),
                "Newest" => books.OrderByDescending(b => b.PublicationYear).ToList(),
                "Title A-Z" => books.OrderBy(b => b.Title).ToList(),
                "Author A-Z" => books.OrderBy(b => b.Author).ToList(),
                _ => books
            };
        }

        [RelayCommand]
        private async Task NavigateToHome()
        {
            await _navigationService.NavigateToAsync("Home");
        }

        [RelayCommand]
        private Task NavigateToLibrary()
        {
            // Déjà sur la page de bibliothèque, ne rien faire
            return Task.CompletedTask;
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
        private Task ApplyFilters()
        {
            LoadBooks();
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task Search()
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                PageTitle = $"Search Results: {SearchQuery}";
                var books = await _bookService.GetBooksBySearchAsync(SearchQuery);
                Books.Clear();
                foreach (var book in books)
                {
                    Books.Add(new BookViewModel(book, _bookService));
                }
            }
        }

        [RelayCommand]
        private Task NextPage()
        {
            if (HasNextPage)
            {
                CurrentPage++;
                LoadBooks();
            }
            return Task.CompletedTask;
        }

        [RelayCommand]
        private Task PreviousPage()
        {
            if (HasPreviousPage)
            {
                CurrentPage--;
                LoadBooks();
            }
            return Task.CompletedTask;
        }

        [RelayCommand]
        private Task RefreshBooks()
        {
            CurrentPage = 1;
            LoadBooks();
            return Task.CompletedTask;
        }
    }

    public partial class CategoryViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private bool _isSelected;
    }

    public partial class LanguageViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private bool _isSelected;
    }
}
