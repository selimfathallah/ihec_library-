-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Users table
CREATE TABLE Users (
    user_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone_number VARCHAR(20),
    profile_picture_url VARCHAR(512),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_email ON Users(email);

-- StudentProfiles table
CREATE TABLE StudentProfiles (
    student_id UUID PRIMARY KEY REFERENCES Users(user_id) ON DELETE CASCADE,
    level_of_study VARCHAR(50) NOT NULL,
    field_of_study VARCHAR(100) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- AdminProfiles table
CREATE TABLE AdminProfiles (
    admin_id UUID PRIMARY KEY REFERENCES Users(user_id) ON DELETE CASCADE,
    job_title VARCHAR(100) NOT NULL,
    is_approved BOOLEAN NOT NULL DEFAULT FALSE,
    approved_by UUID REFERENCES AdminProfiles(admin_id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Books table
CREATE TABLE Books (
    book_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title VARCHAR(255) NOT NULL,
    author VARCHAR(255) NOT NULL,
    publication_year INTEGER NOT NULL CHECK (publication_year > 0),
    publisher VARCHAR(255) NOT NULL,
    isbn VARCHAR(20) NOT NULL UNIQUE,
    description TEXT,
    category VARCHAR(100) NOT NULL,
    availability_status VARCHAR(50) NOT NULL CHECK (availability_status IN ('available', 'borrowed', 'reserved', 'maintenance')),
    added_by UUID REFERENCES Users(user_id) ON DELETE SET NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_books_title ON Books(title);
CREATE INDEX idx_books_isbn ON Books(isbn);
CREATE INDEX idx_books_category ON Books(category);

-- BookStatistics table
CREATE TABLE BookStatistics (
    book_id UUID PRIMARY KEY REFERENCES Books(book_id) ON DELETE CASCADE,
    borrow_count INTEGER DEFAULT 0 CHECK (borrow_count >= 0),
    view_count INTEGER DEFAULT 0 CHECK (view_count >= 0),
    rating_average DECIMAL(3,2) DEFAULT 0.00 CHECK (rating_average >= 0 AND rating_average <= 5.00),
    comment_count INTEGER DEFAULT 0 CHECK (comment_count >= 0),
    like_count INTEGER DEFAULT 0 CHECK (like_count >= 0),
    last_updated TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- BookBorrowings table
CREATE TABLE BookBorrowings (
    id BIGSERIAL PRIMARY KEY,
    book_id UUID REFERENCES Books(book_id) ON DELETE CASCADE,
    user_id UUID REFERENCES Users(user_id) ON DELETE CASCADE,
    borrow_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    due_date TIMESTAMP WITH TIME ZONE NOT NULL,
    is_returned BOOLEAN NOT NULL DEFAULT FALSE,
    return_date TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT valid_dates CHECK (
        due_date > borrow_date 
        AND (return_date IS NULL OR return_date >= borrow_date)
    ),
    UNIQUE(book_id, user_id, borrow_date)
);

CREATE INDEX idx_bookborrowings_user ON BookBorrowings(user_id);
CREATE INDEX idx_bookborrowings_book ON BookBorrowings(book_id);

-- BookRatings table
CREATE TABLE BookRatings (
    id BIGSERIAL PRIMARY KEY,
    book_id UUID REFERENCES Books(book_id) ON DELETE CASCADE,
    user_id UUID REFERENCES Users(user_id) ON DELETE CASCADE,
    rating INTEGER NOT NULL CHECK (rating BETWEEN 1 AND 5),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(book_id, user_id)
);

CREATE INDEX idx_bookratings_book ON BookRatings(book_id);

-- BookComments table
CREATE TABLE BookComments (
    id BIGSERIAL PRIMARY KEY,
    book_id UUID REFERENCES Books(book_id) ON DELETE CASCADE,
    user_id UUID REFERENCES Users(user_id) ON DELETE CASCADE,
    comment_text TEXT NOT NULL CHECK (length(comment_text) > 0),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_bookcomments_book ON BookComments(book_id);
CREATE INDEX idx_bookcomments_user ON BookComments(user_id);

-- BookLikes table
CREATE TABLE BookLikes (
    id BIGSERIAL PRIMARY KEY,
    book_id UUID REFERENCES Books(book_id) ON DELETE CASCADE,
    user_id UUID REFERENCES Users(user_id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(book_id, user_id)
);

CREATE INDEX idx_booklikes_book ON BookLikes(book_id);
CREATE INDEX idx_booklikes_user ON BookLikes(user_id);

-- BooksOfInterest table
CREATE TABLE BooksOfInterest (
    id BIGSERIAL PRIMARY KEY,
    user_id UUID REFERENCES Users(user_id) ON DELETE CASCADE,
    book_id UUID REFERENCES Books(book_id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, book_id)
);

CREATE INDEX idx_booksofinterest_user ON BooksOfInterest(user_id);
CREATE INDEX idx_booksofinterest_book ON BooksOfInterest(book_id);

-- ActivityLogs table
CREATE TABLE ActivityLogs (
    id BIGSERIAL PRIMARY KEY,
    user_id UUID REFERENCES Users(user_id) ON DELETE SET NULL,
    activity_type VARCHAR(50) NOT NULL,
    activity_description TEXT NOT NULL,
    ip_address VARCHAR(45),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_activitylogs_user ON ActivityLogs(user_id);
CREATE INDEX idx_activitylogs_created_at ON ActivityLogs(created_at);

-- AIChatHistory table
CREATE TABLE AIChatHistory (
    id BIGSERIAL PRIMARY KEY,
    user_id UUID REFERENCES Users(user_id) ON DELETE CASCADE,
    user_message TEXT NOT NULL,
    ai_response TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_aichathistory_user ON AIChatHistory(user_id);
CREATE INDEX idx_aichathistory_created_at ON AIChatHistory(created_at);

-- Create updated_at triggers
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply updated_at triggers
CREATE TRIGGER set_updated_at
BEFORE UPDATE ON Users
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER set_updated_at
BEFORE UPDATE ON StudentProfiles
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER set_updated_at
BEFORE UPDATE ON AdminProfiles
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER set_updated_at
BEFORE UPDATE ON Books
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER set_updated_at
BEFORE UPDATE ON BookComments
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER set_updated_at
BEFORE UPDATE ON BookRatings
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Function to update book statistics
CREATE OR REPLACE FUNCTION update_book_statistics()
RETURNS TRIGGER AS $$
BEGIN
    -- Update the relevant counts in BookStatistics
    UPDATE BookStatistics
    SET 
        borrow_count = (SELECT COUNT(*) FROM BookBorrowings WHERE book_id = NEW.book_id),
        rating_average = (SELECT AVG(rating)::DECIMAL(3,2) FROM BookRatings WHERE book_id = NEW.book_id),
        comment_count = (SELECT COUNT(*) FROM BookComments WHERE book_id = NEW.book_id),
        like_count = (SELECT COUNT(*) FROM BookLikes WHERE book_id = NEW.book_id),
        last_updated = CURRENT_TIMESTAMP
    WHERE book_id = NEW.book_id;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Triggers to maintain book statistics
CREATE TRIGGER update_stats_on_borrow
AFTER INSERT OR UPDATE OR DELETE ON BookBorrowings
FOR EACH ROW
EXECUTE FUNCTION update_book_statistics();

CREATE TRIGGER update_stats_on_rating
AFTER INSERT OR UPDATE OR DELETE ON BookRatings
FOR EACH ROW
EXECUTE FUNCTION update_book_statistics();

CREATE TRIGGER update_stats_on_comment
AFTER INSERT OR UPDATE OR DELETE ON BookComments
FOR EACH ROW
EXECUTE FUNCTION update_book_statistics();

CREATE TRIGGER update_stats_on_like
AFTER INSERT OR UPDATE OR DELETE ON BookLikes
FOR EACH ROW
EXECUTE FUNCTION update_book_statistics();
