using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace IHECLibrary.Services.Models
{
    [Table("Books")]
    public class DbBook : BaseModel
    {
        [PrimaryKey("book_id", shouldInsert: false)]
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

        [Column("category")]
        public string? Category { get; set; }

        [Column("availability_status")]
        public string? AvailabilityStatus { get; set; }

        [Column("added_by")]
        public string? AddedBy { get; set; }

        [Column("book_id")]
        public string? Id { get; set; }

        [Column("cover_image_url")]
        public string? CoverImageUrl { get; set; }

        [Column("available_copies")]
        public int AvailableCopies { get; set; }

        [Column("total_copies")]
        public int TotalCopies { get; set; }

        [Column("likes_count")]
        public int LikesCount { get; set; }

        [Column("language")]
        public string? Language { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    [Table("BookStatistics")]
    public class DbBookStatistics : BaseModel
    {
        [PrimaryKey("book_id")]
        public string? BookId { get; set; }

        [Column("borrow_count")]
        public int BorrowCount { get; set; }

        [Column("view_count")]
        public int ViewCount { get; set; }

        [Column("rating_average")]
        public decimal RatingAverage { get; set; }

        [Column("comment_count")]
        public int CommentCount { get; set; }

        [Column("like_count")]
        public int LikeCount { get; set; }

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }
    }

    [Table("BookBorrowings")]
    public class DbBookBorrowing : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        public long Id { get; set; }

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

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    [Table("BookRatings")]
    public class DbBookRating : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        public long Id { get; set; }

        [Column("book_id")]
        public string? BookId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("rating")]
        public int Rating { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    [Table("BookComments")]
    public class DbBookComment : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        public long Id { get; set; }

        [Column("book_id")]
        public string? BookId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("comment_text")]
        public string? CommentText { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    [Table("BookLikes")]
    public class DbBookLike : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        public long Id { get; set; }

        [Column("book_id")]
        public string? BookId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    [Table("BooksOfInterest")]
    public class DbBookOfInterest : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        public long Id { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("book_id")]
        public string? BookId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    [Table("Borrowings")]
    public class DbBookBorrowingDetails : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
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

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }
        
        [Column("extended_count")]
        public int ExtendedCount { get; set; }
    }

    [Table("Reservations")]
    public class DbReservation : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        public string? Id { get; set; }

        [Column("book_id")]
        public string? BookId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("reservation_date")]
        public DateTime ReservationDate { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    [Table("BookReservations")]
    public class DbBookReservation : BaseModel
    {
        [PrimaryKey("reservation_id", shouldInsert: false)]
        public string? ReservationId { get; set; }

        [Column("book_id")]
        public string? BookId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("reservation_date")]
        public DateTime ReservationDate { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("expiration_date")]
        public DateTime? ExpirationDate { get; set; }
    }

    [Table("Activities")]
    public class Activity : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        public string? Id { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("type")]
        public string? Type { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    [Table("UserProfiles")]
    public class UserProfile : BaseModel
    {
        [PrimaryKey("id")]
        public string? Id { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("level_of_study")]
        public string? LevelOfStudy { get; set; }

        [Column("field_of_study")]
        public string? FieldOfStudy { get; set; }

        [Column("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }

        [Column("is_admin")]
        public bool IsAdmin { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    [Table("AdminProfiles")]
    public class AdminProfile : BaseModel
    {
        [PrimaryKey("admin_id")]
        public string? AdminId { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("job_title")]
        public string? JobTitle { get; set; }

        [Column("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }

        [Column("is_approved")]
        public bool IsApproved { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    [Table("Users")]
    public class User : BaseModel
    {
        [PrimaryKey("id")]
        public string? Id { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}