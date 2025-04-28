using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IHECLibrary.Services;
using IHECLibrary.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Supabase;

namespace IHECLibrary.Tests
{
    // Simple placeholder mock implementations directly in this file
    internal class SimpleMockAuthService : IAuthService
    {
        public Task<AuthResult> SignInAsync(string email, string password) => 
            Task.FromResult(new AuthResult { Success = true, User = new UserModel { Id = "test-user" } });
        public Task<AuthResult> SignInWithGoogleAsync() => 
            Task.FromResult(new AuthResult { Success = true, User = new UserModel { Id = "test-user" } });
        public Task<AuthResult> RegisterAsync(UserRegistrationModel model) => 
            Task.FromResult(new AuthResult { Success = true, User = new UserModel { Id = "test-user" } });
        public Task<AuthResult> RegisterAdminAsync(AdminRegistrationModel model) => 
            Task.FromResult(new AuthResult { Success = true, User = new UserModel { Id = "test-admin" } });
        public Task<bool> SignOutAsync() => Task.FromResult(true);
        public Task<bool> ResetPasswordAsync(string email) => Task.FromResult(true);
        public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword) => Task.FromResult(true);
        public bool IsAuthenticated() => true; // Added implementation for IsAuthenticated
    }

    internal class SimpleMockUserService : IUserService
    {
        public Task<UserModel?> GetCurrentUserAsync() => 
            Task.FromResult<UserModel?>(new UserModel { Id = "test-user", Email = "test@example.com" });
        public Task<UserModel?> GetUserByIdAsync(string userId) => 
            Task.FromResult<UserModel?>(new UserModel { Id = userId, Email = "user@example.com" });
        public Task<List<UserModel>> SearchUsersAsync(string searchQuery) => 
            Task.FromResult(new List<UserModel> { new UserModel { Id = "test-user" } });
        public Task<bool> UpdateUserProfileAsync(UserProfileUpdateModel model) => Task.FromResult(true);
        public Task<string> GetUserRankingAsync(string userId) => Task.FromResult("Gold");
        public Task<UserStatisticsModel> GetUserStatisticsAsync(string userId) => 
            Task.FromResult(new UserStatisticsModel { BorrowedBooksCount = 5, LikedBooksCount = 10 });
    }

    internal class SimpleMockBookService : IBookService
    {
        public Task<List<BookModel>> GetAllBooksAsync() => 
            Task.FromResult(new List<BookModel> { new BookModel { Id = "book-1", Title = "Test Book" } });
        public Task<List<BookModel>> GetBooksBySearchAsync(string searchQuery) => 
            Task.FromResult(new List<BookModel> { new BookModel { Id = "book-1", Title = "Test Book" } });
        public Task<List<BookModel>> GetBooksByCategoryAsync(string category) => 
            Task.FromResult(new List<BookModel> { new BookModel { Id = "book-1", Title = "Test Book" } });
        public Task<List<BookModel>> GetBooksByFiltersAsync(List<string> categories, bool availableOnly, string? language) => 
            Task.FromResult(new List<BookModel> { new BookModel { Id = "book-1", Title = "Test Book" } });
        public Task<List<BookModel>> GetRecommendedBooksAsync() => 
            Task.FromResult(new List<BookModel> { new BookModel { Id = "book-1", Title = "Test Book" } });
        public Task<bool> IsBookAvailableAsync(string bookId) => Task.FromResult(true);
        public Task<bool> BorrowBookAsync(string bookId, DateTime? dueDate = null) => Task.FromResult(true);
        public Task<bool> ReturnBookAsync(string bookId) => Task.FromResult(true);
        public Task<bool> ReserveBookAsync(string bookId) => Task.FromResult(true);
        public Task<bool> CancelReservationAsync(string reservationId) => Task.FromResult(true);
        public Task<List<BookModel>> GetBorrowedBooksAsync(string userId) => 
            Task.FromResult(new List<BookModel> { new BookModel { Id = "book-1", Title = "Test Book" } });
        public Task<List<BookModel>> GetReservedBooksAsync(string userId) => 
            Task.FromResult(new List<BookModel> { new BookModel { Id = "book-1", Title = "Test Book" } });
        public Task<BookModel?> GetBookByIdAsync(string id) => 
            Task.FromResult<BookModel?>(new BookModel { Id = id, Title = "Test Book" });
        public Task<bool> LikeBookAsync(string bookId) => Task.FromResult(true);
        public Task<bool> UnlikeBookAsync(string bookId) => Task.FromResult(true);
        public Task<bool> RateBookAsync(string bookId, int rating, string? comment = null) => Task.FromResult(true);
        public Task<List<BookModel>> GetRealBooksAsync(int page = 1, int pageSize = 10, string? category = null, string? searchQuery = null) => 
            Task.FromResult(new List<BookModel> { new BookModel { Id = "book-1", Title = "Test Book" } });
    }

    internal class SimpleMockChatbotService : IChatbotService
    {
        public Task<ChatbotResponse> GetResponseAsync(string userMessage) => 
            Task.FromResult(new ChatbotResponse { Message = "This is a test response" });
        public Task<List<BookModel>> GetBookRecommendationsAsync(string query) => 
            Task.FromResult(new List<BookModel> { new BookModel { Id = "book-1", Title = "Test Book" } });
        public Task<string> GetResearchAssistanceAsync(string topic) => 
            Task.FromResult("Test research assistance");
        public Task<string> GetLibraryInformationAsync(string query) => 
            Task.FromResult("Test library information");
    }

    internal class SimpleMockAdminService : IAdminService
    {
        public Task<AdminModel?> GetCurrentAdminAsync() => 
            Task.FromResult<AdminModel?>(new AdminModel { Id = "test-admin" });
        public Task<AdminModel?> GetAdminByIdAsync(string adminId) => 
            Task.FromResult<AdminModel?>(new AdminModel { Id = adminId });
        public Task<List<AdminModel>> GetAllAdminsAsync() => 
            Task.FromResult(new List<AdminModel> { new AdminModel { Id = "test-admin" } });
        public Task<bool> ApproveAdminRequestAsync(string adminId) => Task.FromResult(true);
        public Task<bool> RejectAdminRequestAsync(string adminId) => Task.FromResult(true);
        public Task<bool> UpdateAdminProfileAsync(AdminProfileUpdateModel model) => Task.FromResult(true);
        public Task<DashboardDataModel> GetDashboardDataAsync() => 
            Task.FromResult(new DashboardDataModel { TotalBooksCount = 15, TotalUsersCount = 10 });
        public Task<List<BookModel>> GetAllBooksAsync() => 
            Task.FromResult(new List<BookModel> { new BookModel { Id = "book-1", Title = "Test Book" } });
        public Task<bool> AddBookAsync(BookAddModel model) => Task.FromResult(true);
        public Task<bool> UpdateBookAsync(BookUpdateModel model) => Task.FromResult(true);
        public Task<bool> DeleteBookAsync(string bookId) => Task.FromResult(true);
        public Task<List<UserModel>> GetAllUsersAsync() => 
            Task.FromResult(new List<UserModel> { new UserModel { Id = "test-user" } });
        public Task<List<BorrowingModel>> GetAllBorrowingsAsync() => 
            Task.FromResult(new List<BorrowingModel> { new BorrowingModel { Id = "borrow-1" } });
        public Task<List<ReservationModel>> GetAllReservationsAsync() => 
            Task.FromResult(new List<ReservationModel> { new ReservationModel { Id = "res-1" } });
    }

    public class TestManager
    {
        private readonly TestRunner _testRunner;
        private bool _useMockServices = false;

        public TestManager(bool useMockServices = true)
        {
            _useMockServices = useMockServices;
            
            // Use mock services by default for testing to avoid external dependencies
            IAuthService authService;
            IUserService userService;
            IBookService bookService;
            IChatbotService chatbotService;
            IAdminService adminService;

            if (_useMockServices)
            {
                // Use simple inline mock implementations
                authService = new SimpleMockAuthService();
                userService = new SimpleMockUserService();
                bookService = new SimpleMockBookService();
                chatbotService = new SimpleMockChatbotService();
                adminService = new SimpleMockAdminService();
            }
            else
            {
                // For real service implementations, we need the required parameters
                var supabaseUrl = "https://kwsczjtdjexydcbzbpws.supabase.co";
                var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imt3c2N6anRkamV4eWRjYnpicHdzIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDUwNjkyNzMsImV4cCI6MjA2MDY0NTI3M30.xfwy8okepbA3d0yaDCUpUXUyvKYUKR1w7SLW3gam5HM";
                var supabaseOptions = new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = true
                };
                var supabaseClient = new Client(supabaseUrl, supabaseKey, supabaseOptions);
                
                // Create a service provider for the navigation service
                var services = new ServiceCollection();
                var serviceProvider = services.BuildServiceProvider();
                
                // Initialize real service implementations with required parameters
                var navigationService = new NavigationService(serviceProvider);
                authService = new SupabaseAuthService(supabaseClient, navigationService);
                userService = new SupabaseUserService(supabaseClient, authService);
                bookService = new SupabaseBookService(supabaseClient, userService);
                chatbotService = new GeminiChatbotService("AIzaSyAHGzJNWYMGDDsSzpAUFn92XjETHFjQ07c", bookService);
                adminService = new SupabaseAdminService(supabaseClient, authService);
            }
            
            // Initialize the test runner with the services
            _testRunner = new TestRunner(authService, userService, bookService, chatbotService, adminService);
        }
        
        public async Task RunTests(string? logPath = null, int iterations = 1)
        {
            if (!string.IsNullOrEmpty(logPath))
            {
                // Ensure the directory exists
                string? logDirectory = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                
                _testRunner.SetLogPath(logPath);
            }
            
            if (iterations <= 1)
            {
                // Run tests once (no iteration)
                await _testRunner.RunAllTests();
            }
            else
            {
                // Run tests with the specified number of iterations
                for (int i = 1; i <= iterations; i++)
                {
                    await _testRunner.RunAllTests(true, i, iterations);
                }
            }
        }
        
        public void SetMockServices(bool useMockServices)
        {
            _useMockServices = useMockServices;
        }
        
        // Static method to create a TestManager instance with services from IServiceProvider
        public static async Task RunTests(IServiceProvider serviceProvider, int iterations = 1)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                iterations > 1 ? "iterative_test_results.log" : "test_results.log");
            
            // Create and run a test manager instance
            var testManager = new TestManager(false); // Use real services from DI
            await testManager.RunTests(logPath, iterations);
        }
    }
}
