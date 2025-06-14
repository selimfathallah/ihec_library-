using Supabase;
using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Postgrest.Attributes;
using Postgrest.Models;

namespace IHECLibrary.Services.Implementations
{
    // Define our database models to match the schema
    [Table("users")]
    public class DbUserModel : BaseModel
    {
        [PrimaryKey("user_id")]
        public string? UserId { get; set; }
        
        [Column("email")]
        public string? Email { get; set; }
        
        [Column("password_hash")]
        public string? PasswordHash { get; set; }
        
        [Column("first_name")]
        public string? FirstName { get; set; }
        
        [Column("last_name")]
        public string? LastName { get; set; }
        
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }
        
        [Column("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }
    }
    
    [Table("student_profiles")]
    public class DbStudentProfileModel : BaseModel
    {
        [PrimaryKey("student_id")]
        public string? StudentId { get; set; }
        
        [Column("level_of_study")]
        public string? LevelOfStudy { get; set; }
        
        [Column("field_of_study")]
        public string? FieldOfStudy { get; set; }
    }
    
    [Table("admin_profiles")]
    public class DbAdminProfileModel : BaseModel
    {
        [PrimaryKey("admin_id")]
        public string? AdminId { get; set; }
        
        [Column("job_title")]
        public string? JobTitle { get; set; }
        
        [Column("is_approved")]
        public bool IsApproved { get; set; }
        
        [Column("approved_by")]
        public string? ApprovedBy { get; set; }
    }
    
    public class SupabaseAuthService : IAuthService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly INavigationService _navigationService;

        public SupabaseAuthService(Supabase.Client supabaseClient, INavigationService navigationService)
        {
            _supabaseClient = supabaseClient;
            _navigationService = navigationService;
        }

        public async Task<AuthResult> SignInAsync(string email, string password)
        {
            try
            {
                var session = await _supabaseClient.Auth.SignIn(email, password);
                
                if (session != null)
                {
                    var user = await GetUserFromSessionAsync(session);
                    return new AuthResult
                    {
                        Success = true,
                        User = user
                    };
                }
                
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Échec de la connexion. Veuillez vérifier vos identifiants."
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Erreur lors de la connexion: {ex.Message}"
                };
            }
        }

        public async Task<AuthResult> RegisterAsync(UserRegistrationModel model)
        {
            try
            {
                Console.WriteLine("Starting registration process...");
                
                if (model == null)
                {
                    Console.WriteLine("Registration failed: Model is null");
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "Les données d'inscription sont invalides."
                    };
                }
                
                // Detailed logging
                Console.WriteLine($"Registering user: {model.Email}, {model.FirstName} {model.LastName}");
                
                // Créer l'utilisateur dans Supabase Auth
                var signUpOptions = new SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        { "first_name", model.FirstName },
                        { "last_name", model.LastName },
                        { "phone_number", model.PhoneNumber },
                        { "level_of_study", model.LevelOfStudy },
                        { "field_of_study", model.FieldOfStudy }
                    }
                };

                Console.WriteLine("Calling Supabase Auth.SignUp...");
                var session = await _supabaseClient.Auth.SignUp(model.Email, model.Password, signUpOptions);
                Console.WriteLine($"SignUp complete. Session: {(session != null ? "valid" : "null")}, User: {(session?.User != null ? "valid" : "null")}");
                
                if (session != null && session.User != null)
                {
                    Console.WriteLine($"User created with ID: {session.User.Id}");
                    
                    try
                    {
                        // Handle profile picture upload if available
                        string profilePictureUrl = "";
                        if (model.HasProfilePicture && !string.IsNullOrEmpty(model.ProfilePictureData))
                        {
                            try
                            {
                                Console.WriteLine("Uploading profile picture...");
                                
                                // Create a unique filename for the profile picture
                                string fileName = $"profile_{session.User.Id}_{DateTime.UtcNow.Ticks}.jpg";
                                
                                // Convert base64 string to bytes
                                byte[] imageData = Convert.FromBase64String(model.ProfilePictureData);
                                
                                // Upload to Supabase Storage
                                var upload = await _supabaseClient.Storage
                                    .From("profile-pictures")
                                    .Upload(
                                        imageData,
                                        fileName,
                                        new Supabase.Storage.FileOptions 
                                        { 
                                            CacheControl = "3600", 
                                            Upsert = true
                                        });
                                
                                if (!string.IsNullOrEmpty(upload))
                                {
                                    // Get the public URL
                                    profilePictureUrl = _supabaseClient.Storage
                                        .From("profile-pictures")
                                        .GetPublicUrl(fileName);
                                    
                                    Console.WriteLine($"Profile picture uploaded successfully: {profilePictureUrl}");
                                }
                            }
                            catch (Exception picEx)
                            {
                                Console.WriteLine($"Error uploading profile picture: {picEx.Message}");
                                // Continue registration even if image upload fails
                            }
                        }
                        
                        // Insert into Users table
                        Console.WriteLine("Inserting into Users table...");
                        await _supabaseClient.From<DbUserModel>()
                            .Insert(new DbUserModel
                            {
                                UserId = session.User.Id,
                                Email = model.Email,
                                PasswordHash = "", // Password is handled by Supabase Auth
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                PhoneNumber = model.PhoneNumber,
                                ProfilePictureUrl = !string.IsNullOrEmpty(profilePictureUrl) 
                                    ? profilePictureUrl 
                                    : "https://kwsczjtdjexydcbzbpws.supabase.co/storage/v1/object/public/profile-pictures/default-profile.png"
                            });
                        Console.WriteLine("Users table insert successful");
                        
                        // Insert into StudentProfiles table
                        Console.WriteLine("Inserting into StudentProfiles table...");
                        await _supabaseClient.From<DbStudentProfileModel>()
                            .Insert(new DbStudentProfileModel
                            {
                                StudentId = session.User.Id,
                                LevelOfStudy = model.LevelOfStudy,
                                FieldOfStudy = model.FieldOfStudy
                            });
                        Console.WriteLine("StudentProfiles table insert successful");

                        // Explicitly get user to ensure we have all profile data
                        Console.WriteLine("Getting user from session...");
                        var user = await GetUserFromSessionAsync(session);
                        Console.WriteLine($"User retrieved: {(user != null ? "success" : "null")}");
                        
                        if (user != null)
                        {
                            Console.WriteLine("Registration successful, returning success result");
                            return new AuthResult
                            {
                                Success = true,
                                User = user
                            };
                        }
                        else
                        {
                            Console.WriteLine("User object is null after GetUserFromSessionAsync");
                            // Even if we couldn't get the full user profile, the registration was successful
                            // This ensures the user can still proceed to the home page
                            return new AuthResult
                            {
                                Success = true,
                                User = new UserModel
                                {
                                    Id = session.User.Id ?? "",
                                    Email = model.Email,
                                    FirstName = model.FirstName,
                                    LastName = model.LastName,
                                    PhoneNumber = model.PhoneNumber,
                                    LevelOfStudy = model.LevelOfStudy,
                                    FieldOfStudy = model.FieldOfStudy,
                                    ProfilePictureUrl = !string.IsNullOrEmpty(profilePictureUrl) 
                                        ? profilePictureUrl 
                                        : "https://kwsczjtdjexydcbzbpws.supabase.co/storage/v1/object/public/profile-pictures/default-profile.png"
                                }
                            };
                        }
                    }
                    catch (Exception dbEx)
                    {
                        Console.WriteLine($"Database error during registration: {dbEx.Message}");
                        // Registration in Auth was successful, but database insert failed
                        // We should still return success to allow the user to proceed
                        return new AuthResult
                        {
                            Success = true,
                            User = new UserModel
                            {
                                Id = session.User.Id ?? "",
                                Email = model.Email,
                                FirstName = model.FirstName,
                                LastName = model.LastName
                            },
                            Message = "Votre compte a été créé, mais certaines informations de profil n'ont pas pu être enregistrées."
                        };
                    }
                }
                
                Console.WriteLine("Registration failed: Session or User is null");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Échec de l'inscription. Veuillez réessayer."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during registration: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Erreur lors de l'inscription: {ex.Message}"
                };
            }
        }

        public async Task<AuthResult> RegisterAdminAsync(AdminRegistrationModel model)
        {
            try
            {
                if (model == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "Les données d'inscription sont invalides."
                    };
                }
                
                // Créer l'utilisateur dans Supabase Auth
                var signUpOptions = new SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        { "first_name", model.FirstName },
                        { "last_name", model.LastName },
                        { "phone_number", model.PhoneNumber },
                        { "job_title", model.JobTitle }
                    }
                };

                var session = await _supabaseClient.Auth.SignUp(model.Email, model.Password, signUpOptions);
                
                if (session != null && session.User != null)
                {
                    // Insert into Users table first
                    await _supabaseClient.From<DbUserModel>()
                        .Insert(new DbUserModel
                        {
                            UserId = session.User.Id,
                            Email = model.Email,
                            PasswordHash = "", // Password is handled by Supabase Auth
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            PhoneNumber = model.PhoneNumber,
                            ProfilePictureUrl = ""
                        });
                        
                    // Then insert into AdminProfiles table
                    await _supabaseClient.From<DbAdminProfileModel>()
                        .Insert(new DbAdminProfileModel
                        {
                            AdminId = session.User.Id,
                            JobTitle = model.JobTitle,
                            IsApproved = false
                        });

                    return new AuthResult
                    {
                        Success = true,
                        Message = "Votre demande d'accès administrateur a été soumise et est en attente d'approbation."
                    };
                }
                
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Échec de l'inscription. Veuillez réessayer."
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Erreur lors de l'inscription: {ex.Message}"
                };
            }
        }

        public Task<AuthResult> SignInWithGoogleAsync()
        {
            try
            {
                // Rediriger vers la page de connexion Google
                var provider = Supabase.Gotrue.Constants.Provider.Google;
                var url = _supabaseClient.Auth.SignIn(provider);
                
                // Ouvrir le navigateur pour la connexion Google
                // Note: Cette partie nécessite une implémentation spécifique pour Avalonia
                // et dépend de la façon dont vous gérez l'authentification OAuth
                
                // Pour l'instant, nous retournons un message d'erreur
                return Task.FromResult(new AuthResult
                {
                    Success = false,
                    ErrorMessage = "La connexion avec Google n'est pas encore implémentée."
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Erreur lors de la connexion avec Google: {ex.Message}"
                });
            }
        }

        public async Task<bool> SignOutAsync()
        {
            try
            {
                await _supabaseClient.Auth.SignOut();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }
                
                await _supabaseClient.Auth.ResetPasswordForEmail(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                if (_supabaseClient.Auth.CurrentUser == null || 
                    string.IsNullOrEmpty(_supabaseClient.Auth.CurrentUser.Email) ||
                    string.IsNullOrEmpty(currentPassword) ||
                    string.IsNullOrEmpty(newPassword))
                {
                    return false;
                }
                
                // Vérifier d'abord le mot de passe actuel
                var session = await _supabaseClient.Auth.SignIn(_supabaseClient.Auth.CurrentUser.Email, currentPassword);
                
                if (session != null)
                {
                    // Changer le mot de passe
                    await _supabaseClient.Auth.Update(new UserAttributes { Password = newPassword });
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task<UserModel?> GetUserFromSessionAsync(Session session)
        {
            try
            {
                if (session == null || session.User == null)
                {
                    return null;
                }
                
                // Récupérer les informations de base de l'utilisateur
                var user = await _supabaseClient.From<DbUserModel>()
                    .Where(p => p.UserId == session.User.Id)
                    .Single();

                if (user == null)
                {
                    return new UserModel
                    {
                        Id = session.User.Id ?? "",
                        Email = session.User.Email ?? "",
                        IsAdmin = false
                    };
                }

                // Vérifier si l'utilisateur est un étudiant (peut être null)
                var studentProfile = await _supabaseClient.From<DbStudentProfileModel>()
                    .Where(p => p.StudentId == session.User.Id)
                    .Single();

                // Vérifier si l'utilisateur est un administrateur (peut être null)
                var adminProfile = await _supabaseClient.From<DbAdminProfileModel>()
                    .Where(p => p.AdminId == session.User.Id)
                    .Single();

                // Créer le modèle utilisateur avec les informations disponibles
                var userModel = new UserModel
                {
                    Id = user.UserId ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    IsAdmin = adminProfile != null && adminProfile.IsApproved
                };

                // Ajouter les informations spécifiques aux étudiants si disponibles
                if (studentProfile != null)
                {
                    userModel.LevelOfStudy = studentProfile.LevelOfStudy;
                    userModel.FieldOfStudy = studentProfile.FieldOfStudy;
                }

                // Si c'est un admin, ajouter les informations spécifiques
                if (adminProfile != null)
                {
                    userModel.JobTitle = adminProfile.JobTitle;
                    userModel.IsApproved = adminProfile.IsApproved;
                }

                return userModel;
            }
            catch
            {
                // Créer un modèle utilisateur de base en cas d'erreur
                if (session?.User != null)
                {
                    return new UserModel
                    {
                        Id = session.User.Id ?? "",
                        Email = session.User.Email ?? "",
                        IsAdmin = false
                    };
                }
                return null;
            }
        }
    }
}
