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

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError = false;

        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;
        private readonly IChatbotService _chatbotService;
        private readonly IBookService _bookService;

        public ChatbotViewModel(INavigationService navigationService, IUserService userService, IChatbotService chatbotService, IBookService bookService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _chatbotService = chatbotService ?? throw new ArgumentNullException(nameof(chatbotService));
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));

            try
            {
                LoadUserData();
                InitializeChat();
            }
            catch (Exception ex)
            {
                // Log the error and show it to the user
                Console.WriteLine($"Error initializing ChatbotViewModel: {ex.Message}");
                ErrorMessage = $"Une erreur s'est produite lors de l'initialisation du chatbot: {ex.Message}";
                HasError = true;
            }
        }

        private async void LoadUserData()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user != null)
                {
                    UserFullName = $"{user.FirstName} {user.LastName}";
                    UserProfilePicture = user.ProfilePictureUrl ?? "/Assets/default_profile.png";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user data: {ex.Message}");
                // Don't fail the entire view for user data loading issue
                UserFullName = "Utilisateur";
                UserProfilePicture = "/Assets/default_profile.png";
            }
        }

        private void InitializeChat()
        {
            try
            {
                // Ajouter un message de bienvenue du chatbot
                var welcomeMessage = new ChatMessageViewModel
                {
                    SenderName = "HEC 1.0",
                    Content = "Bonjour ! Je suis HEC 1.0, l'assistant virtuel de la bibliothèque IHEC Carthage. Comment puis-je vous aider aujourd'hui ?",
                    IsFromBot = true,
                    MessageBackground = "#E6F2F8",
                    MessageAlignment = "Left"
                };

                // Ajouter des suggestions par défaut
                welcomeMessage.Suggestions = new ObservableCollection<string>
                {
                    "Recommande-moi des livres",
                    "Comment emprunter un livre ?",
                    "Quels sont les horaires de la bibliothèque ?",
                    "Aide-moi avec ma recherche"
                };

                Messages.Add(welcomeMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing chat: {ex.Message}");
                throw; // Rethrow to be caught by the constructor
            }
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            try
            {
                // Ignorer les messages vides
                if (string.IsNullOrWhiteSpace(CurrentMessage))
                    return;

                // Ajouter le message de l'utilisateur à la conversation
                var userMessage = new ChatMessageViewModel
                {
                    SenderName = UserFullName,
                    Content = CurrentMessage,
                    IsFromBot = false,
                    MessageBackground = "#FFFFFF",
                    MessageAlignment = "Right"
                };

                Messages.Add(userMessage);

                // Sauvegarder et effacer le message courant
                var messageToSend = CurrentMessage;
                CurrentMessage = string.Empty;

                // Obtenir la réponse du chatbot
                var botResponse = await _chatbotService.GetResponseAsync(messageToSend);

                // Créer le message du chatbot
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                
                // Add error message to chat
                var errorMessage = new ChatMessageViewModel
                {
                    SenderName = "HEC 1.0",
                    Content = "Je suis désolé, une erreur s'est produite lors de l'envoi du message. Veuillez réessayer.",
                    IsFromBot = true,
                    MessageBackground = "#E6F2F8",
                    MessageAlignment = "Left"
                };
                
                Messages.Add(errorMessage);
            }
        }

        [RelayCommand]
        private void UseSuggestion(string suggestion)
        {
            try
            {
                CurrentMessage = suggestion;
                SendMessageCommand.Execute(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error using suggestion: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToHome()
        {
            try
            {
                await _navigationService.NavigateToAsync("Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to Home: {ex.Message}");
                ErrorMessage = $"Erreur de navigation: {ex.Message}";
                HasError = true;
            }
        }

        [RelayCommand]
        private async Task NavigateToLibrary()
        {
            try
            {
                await _navigationService.NavigateToAsync("Library");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to Library: {ex.Message}");
                ErrorMessage = $"Erreur de navigation: {ex.Message}";
                HasError = true;
            }
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
            try
            {
                await _navigationService.NavigateToAsync("Profile");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to Profile: {ex.Message}");
                ErrorMessage = $"Erreur de navigation: {ex.Message}";
                HasError = true;
            }
        }

        [RelayCommand]
        private async Task Search()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    await _navigationService.NavigateToAsync("Library", new LibraryFilterOptions { SearchQuery = SearchQuery });
                    SearchQuery = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching: {ex.Message}");
                ErrorMessage = $"Erreur de recherche: {ex.Message}";
                HasError = true;
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
        private string _messageBackground = "#FFFFFF";

        [ObservableProperty]
        private string _messageAlignment = "Right";

        [ObservableProperty]
        private ObservableCollection<string>? _suggestions;

        [ObservableProperty]
        private ObservableCollection<BookSearchResultViewModel>? _searchResults;

        public bool HasSuggestions => Suggestions != null && Suggestions.Count > 0;
        public bool HasSearchResults => SearchResults != null && SearchResults.Count > 0;
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
            try
            {
                await _navigationService.NavigateToAsync("BookDetails", Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error viewing book: {ex.Message}");
            }
        }
    }
}
