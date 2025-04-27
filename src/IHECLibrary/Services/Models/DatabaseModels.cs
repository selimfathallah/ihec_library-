using System;
using System.Collections.Generic;
using Postgrest.Attributes;
using Postgrest.Models;

namespace IHECLibrary.Services.Models
{
    // Book-related models
    [Table("books")]
    public class DbBook : BaseModel
    {
        [PrimaryKey("book_id")]
        public string? BookId { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("author")]
        public string? Author { get; set; }

        [Column("publication_year")]
        public int PublicationYear { get; set; }

        [Column("publisher")]
        public string? Publisher { get; set; }

        [Column("isbn")]
        public string? ISBN { get; set; }

        [Column("description")]
        public string? Description { get; set; }
        
        [Column("cover_image_url")]
        public string? CoverImageUrl { get; set; }
        
        [Column("page_count")]
        public int? PageCount { get; set; }
        
        [Column("language")]
        public string? Language { get; set; }

        [Column("category")]
        public string? Category { get; set; }
        
        [Column("tags")]
        public string[]? Tags { get; set; }

        [Column("availability_status")]
        public string? AvailabilityStatus { get; set; }
        
        [Column("added_at")]
        public DateTime? AddedAt { get; set; }
        
        [Column("added_by")]
        public string? AddedBy { get; set; }
    }

    [Table("book_statistics")]
    public class DbBookStatistics : BaseModel
    {
        [PrimaryKey("book_id")]
        public string? BookId { get; set; }

        [Column("total_borrows")]
        public int TotalBorrows { get; set; }

        [Column("total_reservations")]
        public int TotalReservations { get; set; }

        [Column("average_rating")]
        public decimal AverageRating { get; set; }

        [Column("total_ratings")]
        public int TotalRatings { get; set; }

        [Column("total_likes")]
        public int TotalLikes { get; set; }
    }

    [Table("book_borrowings")]
    public class DbBookBorrowing : BaseModel
    {
        [PrimaryKey("borrowing_id")]
        public string? Id { get; set; }

        [Column("book_id")]
        public string? BookId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("borrow_date")]
        public DateTime BorrowDate { get; set; }

        [Column("due_date")]
        public DateTime DueDate { get; set; }

        [Column("is_returned")]
        public bool IsReturned { get; set; }

        [Column("return_date")]
        public DateTime? ReturnDate { get; set; }
        
        [Column("reminder_sent")]
        public bool ReminderSent { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    [Table("book_ratings")]
    public class DbBookRating : BaseModel
    {
        [PrimaryKey("rating_id")]
        public string? Id { get; set; }

        [Column("book_id")]
        public string? BookId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("rating")]
        public int Rating { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    [Table("book_comments")]
    public class DbBookComment : BaseModel
    {
        [PrimaryKey("comment_id")]
        public string? Id { get; set; }

        [Column("book_id")]
        public string? BookId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("comment_text")]
        public string? CommentText { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    [Table("book_likes")]
    public class DbBookLike : BaseModel
    {
        [PrimaryKey("like_id")]
        public string? Id { get; set; }

        [Column("book_id")]
        public string? BookId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    [Table("books_of_interest")]
    public class DbBookOfInterest : BaseModel
    {
        [PrimaryKey("interest_id")]
        public string? Id { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("book_id")]
        public string? BookId { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    [Table("book_borrowing_details")]
    public class DbBookBorrowingDetails : BaseModel
    {
        [PrimaryKey("borrowing_id")]
        public string? Id { get; set; }

        [Column("book_id")]
        public string? BookId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("borrow_date")]
        public DateTime BorrowDate { get; set; }

        [Column("due_date")]
        public DateTime DueDate { get; set; }

        [Column("is_returned")]
        public bool IsReturned { get; set; }

        [Column("return_date")]
        public DateTime? ReturnDate { get; set; }
    }

    [Table("book_reservations")]
    public class DbBookReservation : BaseModel
    {
        [PrimaryKey("reservation_id")]
        public string? ReservationId { get; set; }

        [Column("book_id")]
        public string? BookId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("reservation_date")]
        public DateTime ReservationDate { get; set; }

        [Column("status")]
        public string? Status { get; set; }
        
        [Column("notification_sent")]
        public bool NotificationSent { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; }
    }

    // User-related models
    [Table("users")]
    public class DbUser : BaseModel
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
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [Column("last_login")]
        public DateTime? LastLogin { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; }
    }

    [Table("student_profiles")]
    public class DbStudentProfile : BaseModel
    {
        [PrimaryKey("student_id")]
        public string? StudentId { get; set; }

        [Column("level_of_study")]
        public string? LevelOfStudy { get; set; }

        [Column("field_of_study")]
        public string? FieldOfStudy { get; set; }
        
        [Column("books_borrowed")]
        public int BooksBorrowed { get; set; }
        
        [Column("books_reserved")]
        public int BooksReserved { get; set; }
        
        [Column("ranking")]
        public string? Ranking { get; set; }
    }

    [Table("admin_profiles")]
    public class DbAdminProfile : BaseModel
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

    // Added missing model classes

    // This class replaces the missing AdminProfile class
    [Table("admin_profiles")]
    public class AdminProfile : BaseModel
    {
        [PrimaryKey("admin_id")]
        public string? AdminId { get; set; }

        [Column("job_title")]
        public string? JobTitle { get; set; }

        [Column("is_approved")]
        public bool IsApproved { get; set; }

        [Column("approved_by")]
        public string? ApprovedBy { get; set; }
        
        // Extra properties from UserProfile that are used in the code
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // This class replaces the missing UserProfile class
    [Table("users")]
    public class UserProfile : BaseModel
    {
        [PrimaryKey("user_id")]
        public string? Id { get; set; }
        
        [Column("email")]
        public string? Email { get; set; }
        
        [Column("first_name")]
        public string? FirstName { get; set; }
        
        [Column("last_name")]
        public string? LastName { get; set; }
        
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }
        
        [Column("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        // Additional properties that are used in the code
        public string? LevelOfStudy { get; set; }
        public string? FieldOfStudy { get; set; }
        public bool IsAdmin { get; set; }
    }

    // This class replaces the missing DbReservation class
    [Table("book_reservations")]
    public class DbReservation : BaseModel
    {
        [PrimaryKey("reservation_id")]
        public string? Id { get; set; }
        
        [Column("book_id")]
        public string? BookId { get; set; }
        
        [Column("user_id")]
        public string? UserId { get; set; }
        
        [Column("reservation_date")]
        public DateTime ReservationDate { get; set; }
        
        [Column("status")]
        public string? Status { get; set; }
    }

    // This class replaces the missing Activity class
    [Table("activitylogs")]
    public class Activity : BaseModel
    {
        [PrimaryKey("log_id")]
        public string? Id { get; set; }
        
        [Column("user_id")]
        public string? UserId { get; set; }
        
        [Column("activity_type")]
        public string? Type { get; set; }
        
        [Column("activity_description")]
        public string? Description { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        // Additional property used in the code that's not in the DB schema
        public string? Title { get; set; }
    }

    // This class replaces the missing User class
    [Table("users")]
    public class User : BaseModel
    {
        [PrimaryKey("user_id")]
        public string? Id { get; set; }
        
        [Column("email")]
        public string? Email { get; set; }
    }
}