using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using IHECLibrary; // Added import for BookModel

namespace IHECLibrary.ViewModels
{
    public partial class ChatbotViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _userFullName = string.Empty;

        [ObservableProperty]
        private string _userProfilePicture = string.Empty;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private string _currentMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ChatMessageViewModel> _messages = new();

        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;
        private readonly IChatbotService _chatbotService;
        private readonly IBookService _bookService;

        public ChatbotViewModel(INavigationService navigationService, IUserService userService, IChatbotService chatbotService, IBookService bookService)
        {
            _navigationService = navigationService;
            _userService = userService;
            _chatbotService = chatbotService;
            _bookService = bookService;

            LoadUserData();
            InitializeChat();
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

        private void InitializeChat()
        {
            // Message de bienvenue initial
            Messages.Add(new ChatMessageViewModel
            {
                SenderName = "HEC 1.0",
                Content = "Bonjour ! Je suis HEC 1.0, votre assistant de bibliothèque. Comment puis-je vous aider aujourd'hui ?",
                IsFromBot = true,
                MessageBackground = "#E6F2F8",
                MessageAlignment = "Left",
                Suggestions = new ObservableCollection<string>
                {
                    "Recommande-moi des livres",
                    "Comment emprunter un livre ?",
                    "Quels sont les horaires de la bibliothèque ?",
                    "Aide-moi avec ma recherche"
                }
            });
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(CurrentMessage))
                return;

            // Ajouter le message de l'utilisateur
            Messages.Add(new ChatMessageViewModel
            {
                SenderName = UserFullName,
                Content = CurrentMessage,
                IsFromBot = false,
                MessageBackground = "#DCF8C6",
                MessageAlignment = "Right"
            });

            string userMessage = CurrentMessage;
            CurrentMessage = string.Empty;

            // Simuler la réponse du chatbot (à remplacer par l'intégration réelle avec Gemini)
            await Task.Delay(1000); // Simuler un délai de traitement

            // Exemple de réponse du chatbot
            var botResponse = await _chatbotService.GetResponseAsync(userMessage);
            
            var botMessage = new ChatMessageViewModel
            {
                SenderName = "HEC 1.0",
                Content = botResponse.Message,
                IsFromBot = true,
                MessageBackground = "#E6F2F8",
                MessageAlignment = "Left"
            };

            // Ajouter des suggestions si disponibles
            if (botResponse.Suggestions != null && botResponse.Suggestions.Count > 0)
            {
                botMessage.Suggestions = new ObservableCollection<string>(botResponse.Suggestions);
            }

            // Ajouter des résultats de recherche si disponibles
            if (botResponse.BookRecommendations != null && botResponse.BookRecommendations.Count > 0)
            {
                botMessage.SearchResults = new ObservableCollection<BookSearchResultViewModel>();
                foreach (var book in botResponse.BookRecommendations)
                {
                    botMessage.SearchResults.Add(new BookSearchResultViewModel(book, _navigationService));
                }
            }

            Messages.Add(botMessage);
        }

        [RelayCommand]
        private void UseSuggestion(string suggestion)
        {
            CurrentMessage = suggestion;
            SendMessageCommand.Execute(null);
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
        private Task NavigateToChatbot()
        {
            // Déjà sur la page du chatbot, ne rien faire
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task NavigateToProfile()
        {
            await _navigationService.NavigateToAsync("Profile");
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

    public partial class ChatMessageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _senderName = string.Empty;

        [ObservableProperty]
        private string _content = string.Empty;

        [ObservableProperty]
        private bool _isFromBot;

        [ObservableProperty]
        private string _messageBackground = string.Empty;

        [ObservableProperty]
        private string _messageAlignment = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _suggestions = new();

        [ObservableProperty]
        private ObservableCollection<BookSearchResultViewModel> _searchResults = new();

        public bool HasSuggestions => Suggestions.Count > 0;
        public bool HasSearchResults => SearchResults.Count > 0;
    }

    public partial class BookSearchResultViewModel : ViewModelBase
    {
        public string Id { get; }
        public string Title { get; }
        public string Author { get; }

        private readonly INavigationService _navigationService;

        public BookSearchResultViewModel(BookModel book, INavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            Id = book.Id;
            Title = book.Title;
            Author = book.Author;
        }

        [RelayCommand]
        private async Task ViewBook()
        {
            await _navigationService.NavigateToAsync("BookDetails", Id);
        }
    }
}
