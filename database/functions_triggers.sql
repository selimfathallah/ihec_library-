-- Fonctions et déclencheurs pour la base de données IHEC Library

-- Fonction pour mettre à jour la date de dernière modification
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Déclencheurs pour mettre à jour la date de dernière modification
CREATE TRIGGER update_users_updated_at
BEFORE UPDATE ON users
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_books_updated_at
BEFORE UPDATE ON books
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_borrows_updated_at
BEFORE UPDATE ON borrows
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_reservations_updated_at
BEFORE UPDATE ON reservations
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_book_ratings_updated_at
BEFORE UPDATE ON book_ratings
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Fonction pour mettre à jour le nombre d'exemplaires disponibles lors d'un emprunt
CREATE OR REPLACE FUNCTION update_available_copies_on_borrow()
RETURNS TRIGGER AS $$
BEGIN
    -- Si c'est un nouvel emprunt (non retourné)
    IF NEW.is_returned = FALSE AND (TG_OP = 'INSERT' OR (TG_OP = 'UPDATE' AND OLD.is_returned = TRUE)) THEN
        UPDATE books SET available_copies = available_copies - 1 WHERE id = NEW.book_id;
    -- Si un livre est retourné
    ELSIF NEW.is_returned = TRUE AND (OLD IS NULL OR OLD.is_returned = FALSE) THEN
        UPDATE books SET available_copies = available_copies + 1 WHERE id = NEW.book_id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Déclencheur pour mettre à jour le nombre d'exemplaires disponibles
CREATE TRIGGER update_available_copies
AFTER INSERT OR UPDATE ON borrows
FOR EACH ROW EXECUTE FUNCTION update_available_copies_on_borrow();

-- Fonction pour vérifier les réservations lorsqu'un livre est retourné
CREATE OR REPLACE FUNCTION check_reservations_on_return()
RETURNS TRIGGER AS $$
DECLARE
    next_reservation RECORD;
BEGIN
    -- Si un livre vient d'être retourné
    IF NEW.is_returned = TRUE AND (OLD IS NULL OR OLD.is_returned = FALSE) THEN
        -- Chercher la prochaine réservation active pour ce livre
        SELECT * INTO next_reservation FROM reservations 
        WHERE book_id = NEW.book_id AND is_active = TRUE AND is_notified = FALSE
        ORDER BY reservation_date ASC LIMIT 1;
        
        -- Si une réservation est trouvée, marquer comme notifiée
        IF next_reservation.id IS NOT NULL THEN
            UPDATE reservations SET is_notified = TRUE WHERE id = next_reservation.id;
            -- Ici, on pourrait ajouter un appel à une fonction externe pour envoyer un email
        END IF;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Déclencheur pour vérifier les réservations
CREATE TRIGGER check_reservations
AFTER UPDATE ON borrows
FOR EACH ROW EXECUTE FUNCTION check_reservations_on_return();

-- Fonction pour calculer le classement des utilisateurs
CREATE OR REPLACE FUNCTION get_user_ranking(user_id UUID)
RETURNS VARCHAR AS $$
DECLARE
    borrow_count INTEGER;
    user_rank VARCHAR;
BEGIN
    -- Compter le nombre de livres empruntés par l'utilisateur
    SELECT COUNT(*) INTO borrow_count FROM borrows WHERE user_id = get_user_ranking.user_id;
    
    -- Déterminer le classement
    IF borrow_count < 2 THEN
        user_rank := 'Bronze';
    ELSIF borrow_count BETWEEN 2 AND 4 THEN
        user_rank := 'Silver';
    ELSIF borrow_count BETWEEN 5 AND 10 THEN
        user_rank := 'Gold';
    ELSE
        user_rank := 'Master';
    END IF;
    
    RETURN user_rank;
END;
$$ LANGUAGE plpgsql;

-- Fonction pour obtenir des recommandations de livres pour un utilisateur
CREATE OR REPLACE FUNCTION get_book_recommendations(user_id UUID, limit_count INTEGER DEFAULT 10)
RETURNS TABLE (
    book_id UUID,
    title VARCHAR,
    author VARCHAR,
    category VARCHAR,
    cover_image_url VARCHAR,
    available_copies INTEGER
) AS $$
BEGIN
    RETURN QUERY
    WITH user_preferences AS (
        -- Livres aimés par l'utilisateur
        SELECT b.category, COUNT(*) as category_count
        FROM book_likes bl
        JOIN books b ON bl.book_id = b.id
        WHERE bl.user_id = get_book_recommendations.user_id
        GROUP BY b.category
    ),
    user_field AS (
        -- Domaine d'étude de l'utilisateur
        SELECT field_of_study FROM users WHERE id = get_book_recommendations.user_id
    )
    SELECT b.id, b.title, b.author, b.category, b.cover_image_url, b.available_copies
    FROM books b
    LEFT JOIN borrows br ON b.id = br.book_id AND br.user_id = get_book_recommendations.user_id
    WHERE 
        -- Exclure les livres déjà empruntés par l'utilisateur
        br.id IS NULL
        AND (
            -- Inclure les livres de la même catégorie que ceux aimés par l'utilisateur
            b.category IN (SELECT category FROM user_preferences)
            -- Ou les livres liés au domaine d'étude de l'utilisateur
            OR b.category = (SELECT field_of_study FROM user_field)
        )
        -- Et qui sont disponibles
        AND b.available_copies > 0
    ORDER BY 
        -- Prioriser les catégories les plus aimées
        (SELECT COALESCE(category_count, 0) FROM user_preferences up WHERE up.category = b.category) DESC,
        -- Puis par date d'ajout (les plus récents d'abord)
        b.created_at DESC
    LIMIT limit_count;
END;
$$ LANGUAGE plpgsql;
