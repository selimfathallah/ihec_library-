# Documentation Technique - IHEC Library

## Table des matières
1. [Architecture de l'application](#architecture-de-lapplication)
2. [Structure de la base de données](#structure-de-la-base-de-données)
3. [Intégration des APIs](#intégration-des-apis)
4. [Sécurité](#sécurité)
5. [Journalisation et débogage](#journalisation-et-débogage)
6. [Tests](#tests)
7. [Performances](#performances)

## Architecture de l'application

### Vue d'ensemble

L'application IHEC Library est construite selon l'architecture MVVM (Model-View-ViewModel) qui sépare clairement l'interface utilisateur, la logique métier et les données. Cette architecture facilite la maintenance, les tests et l'évolution de l'application.

### Structure des dossiers

```
IHECLibrary/
├── Models/                 # Modèles de données
├── ViewModels/             # ViewModels pour la logique de présentation
├── Views/                  # Vues XAML pour l'interface utilisateur
├── Services/               # Services pour la logique métier
│   ├── Interfaces/         # Interfaces des services
│   └── Implementations/    # Implémentations concrètes des services
├── Tests/                  # Tests unitaires et fonctionnels
└── Assets/                 # Ressources (images, icônes, etc.)
```

### Composants principaux

#### Modèles (Models)

Les modèles représentent les données de l'application et sont généralement des classes POCO (Plain Old CLR Objects) qui ne contiennent pas de logique métier complexe.

Principaux modèles :
- `UserModel` : Représente un utilisateur de l'application
- `BookModel` : Représente un livre dans la bibliothèque
- `BorrowingModel` : Représente un emprunt de livre
- `ReservationModel` : Représente une réservation de livre
- `AdminModel` : Représente un administrateur

#### Vues (Views)

Les vues sont définies en XAML et représentent l'interface utilisateur. Elles sont responsables uniquement de l'affichage et de la capture des interactions utilisateur.

Principales vues :
- `LoginView` : Écran de connexion
- `RegisterView` : Écran d'inscription
- `HomeView` : Page d'accueil avec recommandations
- `LibraryView` : Catalogue de livres
- `ProfileView` : Profil utilisateur
- `ChatbotView` : Interface du chatbot
- `AdminDashboardView` : Tableau de bord administratif

#### ViewModels

Les ViewModels font le lien entre les modèles et les vues. Ils exposent les données des modèles dans un format adapté à l'affichage et gèrent les interactions utilisateur.

Principaux ViewModels :
- `LoginViewModel` : Gère la logique de connexion
- `RegisterViewModel` : Gère la logique d'inscription
- `HomeViewModel` : Gère les recommandations et nouveautés
- `LibraryViewModel` : Gère le catalogue et la recherche
- `ProfileViewModel` : Gère les informations utilisateur
- `ChatbotViewModel` : Gère les interactions avec le chatbot
- `AdminDashboardViewModel` : Gère les fonctionnalités d'administration

#### Services

Les services encapsulent la logique métier et l'accès aux données. Ils sont injectés dans les ViewModels via l'injection de dépendances.

Principaux services :
- `IAuthService` / `SupabaseAuthService` : Gestion de l'authentification
- `IUserService` / `SupabaseUserService` : Gestion des profils utilisateurs
- `IBookService` / `SupabaseBookService` : Gestion des livres et des emprunts
- `IChatbotService` / `GeminiChatbotService` : Intégration avec l'API Gemini
- `IAdminService` / `SupabaseAdminService` : Fonctionnalités d'administration
- `INavigationService` / `NavigationService` : Navigation entre les vues

### Flux de données

1. L'utilisateur interagit avec une vue (par exemple, clique sur un bouton)
2. La vue transmet l'action au ViewModel via une commande
3. Le ViewModel traite l'action, généralement en appelant un ou plusieurs services
4. Les services effectuent les opérations nécessaires (accès à la base de données, appels API, etc.)
5. Les résultats sont renvoyés au ViewModel
6. Le ViewModel met à jour ses propriétés
7. La vue est automatiquement mise à jour grâce au binding de données

## Structure de la base de données

### Supabase

L'application utilise Supabase comme backend pour stocker et gérer les données. Supabase est une alternative open-source à Firebase qui fournit une base de données PostgreSQL, une authentification, un stockage de fichiers et des API RESTful.

### Schéma de la base de données

#### Table `profiles`

Stocke les informations des utilisateurs.

```sql
CREATE TABLE profiles (
    id UUID PRIMARY KEY REFERENCES auth.users(id),
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    phone_number TEXT,
    level_of_study TEXT NOT NULL,
    field_of_study TEXT NOT NULL,
    profile_picture_url TEXT,
    is_admin BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

#### Table `admin_profiles`

Stocke les informations des administrateurs.

```sql
CREATE TABLE admin_profiles (
    id UUID PRIMARY KEY REFERENCES auth.users(id),
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    phone_number TEXT,
    job_title TEXT NOT NULL,
    profile_picture_url TEXT,
    is_approved BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

#### Table `books`

Stocke le catalogue de livres.

```sql
CREATE TABLE books (
    id UUID PRIMARY KEY,
    title TEXT NOT NULL,
    author TEXT NOT NULL,
    isbn TEXT NOT NULL,
    publication_year INTEGER NOT NULL,
    publisher TEXT NOT NULL,
    category TEXT NOT NULL,
    description TEXT,
    cover_image_url TEXT,
    available_copies INTEGER NOT NULL,
    total_copies INTEGER NOT NULL,
    likes_count INTEGER DEFAULT 0,
    language TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

#### Table `borrowings`

Enregistre les emprunts de livres.

```sql
CREATE TABLE borrowings (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES profiles(id),
    book_id UUID NOT NULL REFERENCES books(id),
    borrow_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    due_date TIMESTAMP WITH TIME ZONE NOT NULL,
    return_date TIMESTAMP WITH TIME ZONE,
    is_returned BOOLEAN DEFAULT FALSE
);
```

#### Table `reservations`

Enregistre les réservations de livres.

```sql
CREATE TABLE reservations (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES profiles(id),
    book_id UUID NOT NULL REFERENCES books(id),
    reservation_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    status TEXT NOT NULL DEFAULT 'pending'
);
```

#### Table `book_ratings`

Stocke les évaluations des livres.

```sql
CREATE TABLE book_ratings (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES profiles(id),
    book_id UUID NOT NULL REFERENCES books(id),
    rating INTEGER NOT NULL CHECK (rating BETWEEN 1 AND 5),
    comment TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (user_id, book_id)
);
```

#### Table `book_likes`

Enregistre les "j'aime" sur les livres.

```sql
CREATE TABLE book_likes (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES profiles(id),
    book_id UUID NOT NULL REFERENCES books(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (user_id, book_id)
);
```

#### Table `activities`

Journal des activités pour le tableau de bord administratif.

```sql
CREATE TABLE activities (
    id UUID PRIMARY KEY,
    title TEXT NOT NULL,
    description TEXT NOT NULL,
    type TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

### Fonctions et déclencheurs

#### Mise à jour du nombre de copies disponibles

```sql
CREATE OR REPLACE FUNCTION update_available_copies()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' AND NEW.is_returned = FALSE THEN
        UPDATE books SET available_copies = available_copies - 1 WHERE id = NEW.book_id;
    ELSIF TG_OP = 'UPDATE' AND OLD.is_returned = FALSE AND NEW.is_returned = TRUE THEN
        UPDATE books SET available_copies = available_copies + 1 WHERE id = NEW.book_id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER borrowing_update_available_copies
AFTER INSERT OR UPDATE ON borrowings
FOR EACH ROW
EXECUTE FUNCTION update_available_copies();
```

#### Mise à jour du nombre de "j'aime"

```sql
CREATE OR REPLACE FUNCTION update_likes_count()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        UPDATE books SET likes_count = likes_count + 1 WHERE id = NEW.book_id;
    ELSIF TG_OP = 'DELETE' THEN
        UPDATE books SET likes_count = likes_count - 1 WHERE id = OLD.book_id;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER book_likes_update_count
AFTER INSERT OR DELETE ON book_likes
FOR EACH ROW
EXECUTE FUNCTION update_likes_count();
```

### Politiques de sécurité

Supabase utilise les politiques de sécurité de PostgreSQL (RLS - Row Level Security) pour contrôler l'accès aux données.

#### Politique pour les profils

```sql
CREATE POLICY "Les utilisateurs peuvent voir leur propre profil"
ON profiles FOR SELECT
USING (auth.uid() = id);

CREATE POLICY "Les utilisateurs peuvent mettre à jour leur propre profil"
ON profiles FOR UPDATE
USING (auth.uid() = id);

CREATE POLICY "Les administrateurs peuvent voir tous les profils"
ON profiles FOR SELECT
USING (EXISTS (SELECT 1 FROM admin_profiles WHERE id = auth.uid() AND is_approved = true));
```

#### Politique pour les livres

```sql
CREATE POLICY "Tout le monde peut voir les livres"
ON books FOR SELECT
USING (true);

CREATE POLICY "Seuls les administrateurs peuvent modifier les livres"
ON books FOR INSERT UPDATE DELETE
USING (EXISTS (SELECT 1 FROM admin_profiles WHERE id = auth.uid() AND is_approved = true));
```

## Intégration des APIs

### Supabase

L'application utilise le client Supabase pour C# pour interagir avec la base de données Supabase.

#### Configuration

```csharp
var supabaseUrl = "https://kwsczjtdjexydcbzbpws.supabase.co";
var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imt3c2N6anRkamV4eWRjYnpicHdzIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDUwNjkyNzMsImV4cCI6MjA2MDY0NTI3M30.xfwy8okepbA3d0yaDCUpUXUyvKYUKR1w7SLW3gam5HM";
var supabaseOptions = new SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = true
};
var supabaseClient = new Client(supabaseUrl, supabaseKey, supabaseOptions);
```

#### Authentification

```csharp
// Inscription
var response = await supabaseClient.Auth.SignUp(email, password);

// Connexion
var response = await supabaseClient.Auth.SignIn(email, password);

// Déconnexion
await supabaseClient.Auth.SignOut();
```

#### Requêtes à la base de données

```csharp
// Sélection
var books = await supabaseClient.From<Book>()
    .Select()
    .Order(b => b.Title)
    .Get();

// Insertion
await supabaseClient.From<Book>().Insert(newBook);

// Mise à jour
await supabaseClient.From<Book>()
    .Where(b => b.Id == bookId)
    .Set(b => b.AvailableCopies, newValue)
    .Update();

// Suppression
await supabaseClient.From<Book>()
    .Where(b => b.Id == bookId)
    .Delete();
```

### API Gemini

L'application utilise l'API Gemini de Google pour alimenter le chatbot HEC 1.0.

#### Configuration

```csharp
var geminiApiKey = "AIzaSyAHGzJNWYMGDDsSzpAUFn92XjETHFjQ07c";
```

#### Envoi de requêtes

```csharp
public async Task<ChatbotResponse> GetResponseAsync(string userMessage)
{
    try
    {
        var client = new RestClient($"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}");
        var request = new RestRequest("", Method.Post);
        request.AddHeader("Content-Type", "application/json");

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = userMessage
                        }
                    }
                }
            }
        };

        request.AddJsonBody(JsonConvert.SerializeObject(payload));
        var response = await client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            var responseData = JsonConvert.DeserializeObject<GeminiResponse>(response.Content);
            
            // Traitement de la réponse
            var message = responseData.Candidates[0].Content.Parts[0].Text;
            
            // Extraction des suggestions et recommandations de livres
            var suggestions = ExtractSuggestions(message);
            var bookRecommendations = await ExtractBookRecommendations(message);
            
            return new ChatbotResponse
            {
                Message = message,
                Suggestions = suggestions,
                BookRecommendations = bookRecommendations
            };
        }
        else
        {
            return new ChatbotResponse
            {
                Message = "Désolé, je ne peux pas répondre pour le moment. Veuillez réessayer plus tard."
            };
        }
    }
    catch (Exception ex)
    {
        DebugHelper.LogException(ex, "GeminiChatbotService.GetResponseAsync");
        return new ChatbotResponse
        {
            Message = "Une erreur s'est produite. Veuillez réessayer plus tard."
        };
    }
}
```

## Sécurité

### Authentification

L'application utilise le système d'authentification de Supabase qui est basé sur JWT (JSON Web Tokens). Les mots de passe sont hachés et salés avant d'être stockés.

### Stockage sécurisé des clés API

Les clés API sont stockées dans le code de l'application. Dans un environnement de production, il serait préférable de les stocker dans un fichier de configuration sécurisé ou d'utiliser un gestionnaire de secrets.

### Validation des entrées

L'application utilise FluentValidation pour valider les entrées utilisateur avant de les envoyer à la base de données.

```csharp
public class UserRegistrationValidator : AbstractValidator<UserRegistrationModel>
{
    public UserRegistrationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'adresse e-mail est requise")
            .EmailAddress().WithMessage("L'adresse e-mail n'est pas valide")
            .Must(BeValidDomain).WithMessage("Vous devez utiliser votre adresse e-mail académique (@ihec.ucar.tn)");
            
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Le mot de passe est requis")
            .MinimumLength(8).WithMessage("Le mot de passe doit contenir au moins 8 caractères")
            .Matches("[A-Z]").WithMessage("Le mot de passe doit contenir au moins une lettre majuscule")
            .Matches("[0-9]").WithMessage("Le mot de passe doit contenir au moins un chiffre")
            .Matches("[^a-zA-Z0-9]").WithMessage("Le mot de passe doit contenir au moins un caractère spécial");
            
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Le prénom est requis");
            
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Le nom est requis");
            
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Le numéro de téléphone est requis")
            .Matches(@"^\+?[0-9]{8,15}$").WithMessage("Le numéro de téléphone n'est pas valide");
            
        RuleFor(x => x.LevelOfStudy)
            .NotEmpty().WithMessage("Le niveau d'études est requis");
            
        RuleFor(x => x.FieldOfStudy)
            .NotEmpty().WithMessage("Le domaine d'études est requis");
    }
    
    private bool BeValidDomain(string email)
    {
        return email.EndsWith("@ihec.ucar.tn");
    }
}
```

### Protection contre les injections SQL

Supabase utilise des requêtes paramétrées qui protègent contre les injections SQL.

## Journalisation et débogage

### Configuration de Serilog

L'application utilise Serilog pour la journalisation des événements, des erreurs et des informations de débogage.

```csharp
var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

var logFilePath = Path.Combine(logDirectory, "ihec_library.log");

Serilog.Log.Logger = new Serilog.LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(logFilePath, rollingInterval: Serilog.RollingInterval.Day)
    .CreateLogger();
```

### Journalisation des erreurs

```csharp
try
{
    // Code qui peut générer une exception
}
catch (Exception ex)
{
    Serilog.Log.Error(ex, "Une erreur s'est produite lors de {Operation}", "opération spécifique");
    // Gestion de l'erreur
}
```

### Classe DebugHelper

La classe `DebugHelper` fournit des méthodes pour faciliter le débogage et la journalisation.

```csharp
public static void LogDebugInfo(string message)
{
    try
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string logMessage = $"[{timestamp}] {message}";
        
        File.AppendAllText(DebugLogPath, logMessage + Environment.NewLine);
        Debug.WriteLine(logMessage);
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Erreur lors de la journalisation du débogage: {ex.Message}");
    }
}

public static void LogException(Exception ex, string context = "")
{
    try
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string logMessage = $"[{timestamp}] EXCEPTION {(string.IsNullOrEmpty(context) ? "" : $"dans {context}")}: {ex.Message}";
        logMessage += Environment.NewLine + ex.StackTrace;
        
        if (ex.InnerException != null)
        {
            logMessage += Environment.NewLine + $"Inner Exception: {ex.InnerException.Message}";
            logMessage += Environment.NewLine + ex.InnerException.StackTrace;
        }
        
        File.AppendAllText(DebugLogPath, logMessage + Environment.NewLine);
        Debug.WriteLine(logMessage);
    }
    catch (Exception logEx)
    {
        Debug.WriteLine($"Erreur lors de la journalisation de l'exception: {logEx.Message}");
    }
}
```

## Tests

### Framework de test

L'application inclut un framework de test complet pour vérifier le bon fonctionnement de toutes les fonctionnalités.

#### TestRunner

La classe `TestRunner` exécute des tests fonctionnels sur tous les services de l'application.

```csharp
public async Task RunAllTests()
{
    _logger.Information("=== Début des tests de l'application IHEC Library ===");
    _logger.Information($"Date et heure: {DateTime.Now}");
    
    try
    {
        await TestAuthentication();
        await TestUserService();
        await TestBookService();
        await TestChatbotService();
        await TestAdminService();
        
        _logger.Information("=== Tous les tests ont été exécutés avec succès ===");
    }
    catch (Exception ex)
    {
        _logger.Error($"Une erreur s'est produite pendant les tests: {ex.Message}");
        _logger.Error(ex.StackTrace);
    }
}
```

#### ApplicationTester

La classe `ApplicationTester` vérifie les prérequis système et les connexions aux APIs.

```csharp
public async Task RunSystemTests()
{
    AddTestResult("h1", "Rapport de test de l'application IHEC Library");
    AddTestResult("p", $"Date et heure: {DateTime.Now}");
    
    await TestSystemRequirements();
    await TestApiConnections();
    
    GenerateHtmlReport();
}
```

### Exécution des tests

Les tests peuvent être exécutés en lançant l'application avec l'argument `--test` :

```bash
dotnet run --project src/IHECLibrary/IHECLibrary.csproj -- --test
```

### Rapports de test

Les tests génèrent des rapports détaillés au format HTML et texte qui sont stockés dans le dossier de l'application.

## Performances

### Optimisations

#### Chargement paresseux des données

L'application utilise le chargement paresseux (lazy loading) pour charger les données uniquement lorsqu'elles sont nécessaires, ce qui améliore les performances et réduit la consommation de mémoire.

```csharp
private async Task LoadBooksAsync()
{
    if (_books == null || !_books.Any())
    {
        IsLoading = true;
        try
        {
            var books = await _bookService.GetRecommendedBooksAsync();
            Books = new ObservableCollection<BookModel>(books);
        }
        catch (Exception ex)
        {
            DebugHelper.LogException(ex, "HomeViewModel.LoadBooksAsync");
            // Gérer l'erreur
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

#### Mise en cache

L'application met en cache certaines données pour réduire le nombre d'appels à la base de données.

```csharp
private Dictionary<string, BookModel> _bookCache = new Dictionary<string, BookModel>();

public async Task<BookModel> GetBookByIdAsync(string bookId)
{
    if (_bookCache.TryGetValue(bookId, out var cachedBook))
    {
        return cachedBook;
    }
    
    try
    {
        var book = await _supabaseClient.From<Book>()
            .Where(b => b.Id == bookId)
            .Single();
            
        if (book != null)
        {
            var bookModel = MapToBookModel(book);
            _bookCache[bookId] = bookModel;
            return bookModel;
        }
        
        return null;
    }
    catch
    {
        return null;
    }
}
```

#### Pagination

Pour les listes longues, l'application utilise la pagination pour charger les données par lots.

```csharp
public async Task<List<BookModel>> GetBooksByCategoryAsync(string category, int page = 1, int pageSize = 20)
{
    try
    {
        var books = await _supabaseClient.From<Book>()
            .Where(b => b.Category == category)
            .Range((page - 1) * pageSize, page * pageSize - 1)
            .Order(b => b.Title)
            .Get();
            
        return books.Select(MapToBookModel).ToList();
    }
    catch
    {
        return new List<BookModel>();
    }
}
```

### Gestion de la mémoire

L'application utilise des techniques de gestion de la mémoire pour éviter les fuites de mémoire et optimiser les performances.

```csharp
public void Dispose()
{
    // Libérer les ressources
    _bookCache.Clear();
    _userCache.Clear();
    
    // Annuler les opérations en cours
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource?.Dispose();
    _cancellationTokenSource = null;
    
    // Supprimer les abonnements aux événements
    if (_supabaseClient != null)
    {
        _supabaseClient.Auth.AuthStateChanged -= OnAuthStateChanged;
    }
}
```
