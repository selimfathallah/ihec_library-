using System;
using System.Threading.Tasks;
using IHECLibrary.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IHECLibrary.Tests
{
    public class TestManager
    {
        public static async Task RunTests(IServiceProvider serviceProvider)
        {
            Console.WriteLine("Démarrage des tests de l'application IHEC Library...");
            
            try
            {
                var testRunner = new TestRunner(
                    serviceProvider.GetRequiredService<IAuthService>(),
                    serviceProvider.GetRequiredService<IUserService>(),
                    serviceProvider.GetRequiredService<IBookService>(),
                    serviceProvider.GetRequiredService<IChatbotService>(),
                    serviceProvider.GetRequiredService<IAdminService>()
                );
                
                await testRunner.RunAllTests();
                
                Console.WriteLine("Tests terminés avec succès. Consultez le fichier test_results.log pour plus de détails.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Une erreur s'est produite pendant les tests: {ex.Message}");
            }
        }
    }
}
