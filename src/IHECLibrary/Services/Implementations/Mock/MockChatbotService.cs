using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IHECLibrary.Services;

namespace IHECLibrary.Services.Implementations.Mock
{
    public class MockChatbotService : IChatbotService
    {
        private readonly Random _random = new Random();
        
        public Task<ChatbotResponse> GetResponseAsync(string userMessage)
        {
            var response = new ChatbotResponse();
            
            if (userMessage.Contains("book", StringComparison.OrdinalIgnoreCase) || 
                userMessage.Contains("recommend", StringComparison.OrdinalIgnoreCase))
            {
                response.Message = "Here are some book recommendations that might interest you:";
                response.BookRecommendations = GetSampleBooks();
                response.Suggestions = new List<string>
                {
                    "Tell me more about finance books",
                    "I want to borrow a book",
                    "How do I return a book?"
                };
            }
            else if (userMessage.Contains("hour", StringComparison.OrdinalIgnoreCase) || 
                     userMessage.Contains("open", StringComparison.OrdinalIgnoreCase))
            {
                response.Message = "The library is open Monday to Friday from 8:00 AM to 8:00 PM, and Saturdays from 9:00 AM to 5:00 PM. We are closed on Sundays.";
                response.Suggestions = new List<string>
                {
                    "Do you have a quiet study area?",
                    "How do I reserve a book?",
                    "Can I extend my borrowing period?"
                };
            }
            else if (userMessage.Contains("help", StringComparison.OrdinalIgnoreCase) || 
                     userMessage.Contains("assistance", StringComparison.OrdinalIgnoreCase))
            {
                response.Message = "I'm here to help! You can ask me about books, library services, opening hours, or research assistance. What would you like to know?";
                response.Suggestions = new List<string>
                {
                    "Find books on economics",
                    "How to borrow a book",
                    "Library opening hours",
                    "Research assistance"
                };
            }
            else
            {
                // Default response
                response.Message = "I'm the IHEC Library Assistant. How can I help you today?";
                response.Suggestions = new List<string>
                {
                    "Show me recommended books",
                    "Tell me about library hours",
                    "Help with research",
                    "How to borrow books"
                };
            }
            
            return Task.FromResult(response);
        }

        public Task<List<BookModel>> GetBookRecommendationsAsync(string query)
        {
            var books = GetSampleBooks();
            
            if (!string.IsNullOrEmpty(query))
            {
                books = books.Where(b => 
                    b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                    b.Author.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                    b.Category.Contains(query, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            
            return Task.FromResult(books);
        }

        public Task<string> GetResearchAssistanceAsync(string topic)
        {
            var responses = new Dictionary<string, string>
            {
                { "finance", "For finance research, I recommend checking our collection of journals including the Journal of Finance, Journal of Financial Economics, and Review of Financial Studies." },
                { "marketing", "Our marketing resources include case studies, consumer behavior research, and digital marketing strategy books." },
                { "economics", "For economics, we have a comprehensive collection covering micro and macroeconomics, economic development, and international trade." },
                { "management", "Our management section includes books on organizational behavior, strategic management, and leadership." }
            };
            
            foreach (var key in responses.Keys)
            {
                if (topic.Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(responses[key]);
                }
            }
            
            return Task.FromResult($"For research on {topic}, I recommend visiting our reference section or speaking with our librarian who can provide specialized assistance.");
        }

        public Task<string> GetLibraryInformationAsync(string query)
        {
            var infoResponses = new Dictionary<string, string>
            {
                { "hour", "The library is open Monday to Friday from 8:00 AM to 8:00 PM, and Saturdays from 9:00 AM to 5:00 PM. We are closed on Sundays." },
                { "borrow", "You can borrow up to 5 books at a time for a period of 2 weeks. You'll need your student ID card to check out books." },
                { "return", "Books can be returned to the drop box near the entrance or at the circulation desk during opening hours." },
                { "fine", "Late returns incur a fine of 0.5 dinars per day per book." },
                { "renew", "You can renew books twice, each time for two additional weeks, unless the book has been reserved by another user." }
            };
            
            foreach (var key in infoResponses.Keys)
            {
                if (query.Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(infoResponses[key]);
                }
            }
            
            return Task.FromResult("For information about our library services, please visit the information desk or check our website. Our staff is always ready to assist you.");
        }
        
        private List<BookModel> GetSampleBooks()
        {
            return new List<BookModel>
            {
                new BookModel
                {
                    Id = "1",
                    Title = "Principles of Finance",
                    Author = "John Smith",
                    Category = "Finance",
                    Description = "A comprehensive guide to financial principles and practices.",
                    Language = "English",
                    PublicationYear = 2022,
                    AvailableCopies = 3,
                    TotalCopies = 5
                },
                new BookModel
                {
                    Id = "2",
                    Title = "Marketing Strategies",
                    Author = "Jane Johnson",
                    Category = "Marketing",
                    Description = "Effective marketing strategies for modern businesses.",
                    Language = "English",
                    PublicationYear = 2021,
                    AvailableCopies = 2,
                    TotalCopies = 3
                },
                new BookModel
                {
                    Id = "3",
                    Title = "Economics 101",
                    Author = "Robert Williams",
                    Category = "Economics",
                    Description = "An introduction to basic economic theories and concepts.",
                    Language = "English",
                    PublicationYear = 2020,
                    AvailableCopies = 0,
                    TotalCopies = 2
                },
                new BookModel
                {
                    Id = "4",
                    Title = "Business Analytics",
                    Author = "Lisa Brown",
                    Category = "BI",
                    Description = "Data-driven approaches to business decision-making.",
                    Language = "English",
                    PublicationYear = 2023,
                    AvailableCopies = 1,
                    TotalCopies = 1
                }
            };
        }
    }
}