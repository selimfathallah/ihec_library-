using System.Collections.Generic;
using System.Threading.Tasks;

namespace IHECLibrary.Services
{
    public interface IChatbotService
    {
        Task<ChatbotResponse> GetResponseAsync(string userMessage);
        Task<List<BookModel>> GetBookRecommendationsAsync(string query);
        Task<string> GetResearchAssistanceAsync(string topic);
        Task<string> GetLibraryInformationAsync(string query);
    }

    public class ChatbotResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new List<string>();
        public List<BookModel> BookRecommendations { get; set; } = new List<BookModel>();
    }
}
