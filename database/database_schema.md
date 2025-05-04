# IHEC Library Database Documentation

## Overview

This document provides a comprehensive overview of the database schema for the IHEC Library management system. The system is built using Supabase as the backend database service, with PostgreSQL as the underlying database engine.

## Database Information
- **Project Name**: IHEC_Library
- **Region**: eu-central-1
- **Database Version**: 15.8.1.073

## Entity Relationship Diagram

The database consists of several related tables designed to manage library resources, user interactions, and administrative operations.

### Core Entities:
- Users
- Books
- Admin Profiles
- Book-related activities (borrowing, reservations, ratings, comments, likes)
- Chatbot integration

## Tables Structure

### Users

Primary table that stores information about all users in the system.

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| user_id | uuid | Primary key | NOT NULL, DEFAULT uuid_generate_v4() |
| email | varchar | User email | NOT NULL, UNIQUE, CHECK (email LIKE '%@ihec.ucar.tn') |
| password_hash | varchar | Hashed password | NOT NULL |
| first_name | varchar | User's first name | NOT NULL |
| last_name | varchar | User's last name | NOT NULL |
| phone_number | varchar | User's phone number | NOT NULL, CHECK (phone_number LIKE '+216%') |
| profile_picture_url | text | URL to profile picture | NOT NULL |
| created_at | timestamptz | Account creation date | DEFAULT now() |
| last_login | timestamptz | Most recent login time | NULL |
| is_active | boolean | Account status | DEFAULT true |
| level_of_study | varchar | Academic level | CHECK (level_of_study IN ('1', '2', '3', 'M1', 'M2', 'Other')) |
| field_of_study | varchar | Major/specialization | CHECK (field_of_study IN ('BI', 'Gestion', 'Finance', 'Management', 'Marketing', 'Big Data', 'Accounting', 'Other')) |
| books_borrowed | integer | Count of borrowed books | DEFAULT 0 |
| books_reserved | integer | Count of reserved books | DEFAULT 0 |
| ranking | varchar | User rank in the system | DEFAULT 'Bronze', CHECK (ranking IN ('Bronze', 'Silver', 'Gold', 'Master')) |
| is_student | boolean | Student status flag | DEFAULT true |
| bio | text | User biography | NULL |
| field_interest | text[] | Array of academic interests | DEFAULT '{}' |

### AdminProfiles

Stores additional information about users with administrative privileges.

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| admin_id | uuid | Primary key, references users(user_id) | NOT NULL |
| job_title | varchar | Position title | NOT NULL, CHECK (job_title IN ('Professor', 'Librarian', 'Administration')) |
| is_approved | boolean | Approval status | DEFAULT false |
| approved_by | uuid | References the user who approved | NULL, REFERENCES users(user_id) |

### Books

Contains information about all books in the library.

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| book_id | uuid | Primary key | NOT NULL, DEFAULT uuid_generate_v4() |
| title | varchar | Book title | NOT NULL |
| author | varchar | Book author | NOT NULL |
| publication_year | integer | Year published | NULL |
| publisher | varchar | Publishing house | NULL |
| isbn | varchar | ISBN number | NULL, UNIQUE |
| description | text | Book description | NULL |
| cover_image_url | text | URL to cover image | NULL |
| page_count | integer | Number of pages | NULL |
| language | varchar | Book language | NULL |
| category | varchar | Primary category | NULL |
| tags | text[] | Array of tags/keywords | NULL |
| availability_status | varchar | Current status | DEFAULT 'Available', CHECK (availability_status IN ('Available', 'Borrowed', 'Reserved')) |
| added_at | timestamptz | When book was added | DEFAULT now() |
| added_by | uuid | User who added the book | NULL, REFERENCES users(user_id) |
| copies | integer | Total number of copies | DEFAULT 1, NOT NULL |
| copies_left | integer | Available copies | DEFAULT 1, NOT NULL |

### BookStatistics

Aggregated statistics about each book.

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| book_id | uuid | Primary key, references books(book_id) | NOT NULL |
| total_borrows | integer | Number of times borrowed | DEFAULT 0 |
| total_reservations | integer | Number of times reserved | DEFAULT 0 |
| average_rating | numeric | Average rating value | DEFAULT 0 |
| total_ratings | integer | Number of ratings received | DEFAULT 0 |
| total_likes | integer | Number of likes received | DEFAULT 0 |

### BookBorrowings

Records of book borrowing transactions.

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| borrowing_id | uuid | Primary key | NOT NULL, DEFAULT uuid_generate_v4() |
| book_id | uuid | References books(book_id) | NOT NULL |
| user_id | uuid | References users(user_id) | NOT NULL |
| borrow_date | timestamptz | Date borrowed | NOT NULL, DEFAULT now() |
| due_date | timestamptz | Return deadline | NOT NULL |
| return_date | timestamptz | Actual return date | NULL |
| is_returned | boolean | Return status | DEFAULT false |
| reminder_sent | boolean | If reminder was sent | DEFAULT false |
| created_at | timestamptz | Record creation time | DEFAULT now() |

### BookReservations

Records of book reservation requests.

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| reservation_id | uuid | Primary key | NOT NULL, DEFAULT uuid_generate_v4() |
| book_id | uuid | References books(book_id) | NOT NULL |
| user_id | uuid | References users(user_id) | NOT NULL |
| reservation_date | timestamptz | Date reserved | NOT NULL, DEFAULT now() |
| notification_sent | boolean | If notification was sent | DEFAULT false |
| is_active | boolean | If reservation is active | DEFAULT true |

### BookRatings

User ratings for books.

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| rating_id | uuid | Primary key | NOT NULL, DEFAULT uuid_generate_v4() |
| book_id | uuid | References books(book_id) | NOT NULL |
| user_id | uuid | References users(user_id) | NOT NULL |
| rating | integer | Rating value (1-5) | NOT NULL, CHECK (rating >= 1 AND rating <= 5) |
| created_at | timestamptz | Rating submission time | DEFAULT now() |

### BookComments

User comments on books.

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| comment_id | uuid | Primary key | NOT NULL, DEFAULT uuid_generate_v4() |
| book_id | uuid | References books(book_id) | NOT NULL |
| user_id | uuid | References users(user_id) | NOT NULL |
| comment_text | text | Comment content | NOT NULL |
| created_at | timestamptz | Comment submission time | DEFAULT now() |
| updated_at | timestamptz | Comment update time | NULL |

### BookLikes

Records user "likes" of books.

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| like_id | uuid | Primary key | NOT NULL, DEFAULT uuid_generate_v4() |
| book_id | uuid | References books(book_id) | NOT NULL |
| user_id | uuid | References users(user_id) | NOT NULL |
| created_at | timestamptz | Like submission time | DEFAULT now() |

### BooksOfInterest

Records books users have marked as interesting.

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| interest_id | uuid | Primary key | NOT NULL, DEFAULT uuid_generate_v4() |
| user_id | uuid | References users(user_id) | NOT NULL |
| book_id | uuid | References books(book_id) | NOT NULL |
| created_at | timestamptz | Record creation time | DEFAULT now() |

### AIChatHistory

Stores conversations between users and the AI chatbot.

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| chat_id | uuid | Primary key | NOT NULL, DEFAULT uuid_generate_v4() |
| user_id | uuid | References users(user_id) | NOT NULL |
| user_message | text | User's message | NOT NULL |
| ai_response | text | Chatbot's response | NOT NULL |
| created_at | timestamptz | Message timestamp | DEFAULT now() |

### ActivityLogs

System activity logs for audit and analysis.

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| log_id | uuid | Primary key | NOT NULL, DEFAULT uuid_generate_v4() |
| user_id | uuid | References users(user_id) | NULL |
| activity_type | varchar | Type of activity | NOT NULL |
| activity_description | text | Description of activity | NULL |
| ip_address | varchar | IP address | NULL |
| created_at | timestamptz | Log timestamp | DEFAULT now() |

## Relationships

### Main Relationships:
1. **Users and Books**:
   - Users can borrow books (BookBorrowings)
   - Users can reserve books (BookReservations)
   - Users can rate books (BookRatings)
   - Users can comment on books (BookComments)
   - Users can like books (BookLikes)
   - Users can mark books as interesting (BooksOfInterest)
   - Admins (Users) can add books (Books.added_by)

2. **Administrative Access**:
   - Users can have admin profiles (AdminProfiles)
   - Admins can approve other admins (AdminProfiles.approved_by)

3. **Book Tracking**:
   - Each book has statistics (BookStatistics)
   - Activities related to books are tracked in ActivityLogs

4. **Chatbot Integration**:
   - User interactions with the chatbot are stored (AIChatHistory)

## Security Features

1. **Row Level Security (RLS)**:
   - Enabled on users table
   - Enabled on books table

2. **Data Constraints**:
   - Email validation (must end with @ihec.ucar.tn)
   - Phone number validation (must start with +216)
   - Enum-like constraints on various fields like ranking, level_of_study, etc.
   - Rating values limited to 1-5

## System Design Notes

1. **User Ranking System**:
   The system includes a gamification element with user rankings (Bronze, Silver, Gold, Master).

2. **Book Availability Tracking**:
   - The system tracks both total copies and available copies
   - Availability status transitions (Available, Borrowed, Reserved)

3. **Statistics Collection**:
   Aggregated statistics for books are maintained separately for performance.

4. **Activity Tracking**:
   Comprehensive activity logging for audit and analysis purposes.

5. **AI Integration**:
   Built-in support for AI chatbot functionality with conversation history.

## Usage Patterns

This database structure supports the following key use cases:
- Student registration and profile management
- Admin registration and approval workflows
- Book management (adding, updating inventory)
- Borrowing and reservation processes
- Social features (ratings, comments, likes)
- Interest-based recommendations
- AI-assisted library support
- Activity tracking and analytics

## Best Practices for Queries

When querying this database:
1. Use joins wisely when accessing related tables
2. Consider performance implications for statistics-heavy queries
3. Respect RLS policies when writing custom queries
4. Use parameters for all user-supplied inputs to prevent injection attacks
5. Cache frequently accessed read-only data when appropriate

---

This documentation represents the database schema as of May 4, 2025.