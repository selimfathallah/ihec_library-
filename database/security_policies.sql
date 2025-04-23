-- Règles de sécurité et politiques d'accès pour Supabase

-- Activer l'extension RLS (Row Level Security)
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE books ENABLE ROW LEVEL SECURITY;
ALTER TABLE borrows ENABLE ROW LEVEL SECURITY;
ALTER TABLE reservations ENABLE ROW LEVEL SECURITY;
ALTER TABLE book_ratings ENABLE ROW LEVEL SECURITY;
ALTER TABLE book_likes ENABLE ROW LEVEL SECURITY;
ALTER TABLE chat_history ENABLE ROW LEVEL SECURITY;

-- Politique pour les utilisateurs (users)
-- Les utilisateurs peuvent voir leur propre profil
CREATE POLICY "Users can view their own profile" ON users
    FOR SELECT
    USING (auth.uid() = id);

-- Les utilisateurs peuvent modifier leur propre profil
CREATE POLICY "Users can update their own profile" ON users
    FOR UPDATE
    USING (auth.uid() = id);

-- Les administrateurs peuvent voir tous les profils
CREATE POLICY "Admins can view all profiles" ON users
    FOR SELECT
    USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

-- Les administrateurs peuvent modifier tous les profils
CREATE POLICY "Admins can update all profiles" ON users
    FOR UPDATE
    USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

-- Les administrateurs peuvent supprimer des utilisateurs
CREATE POLICY "Admins can delete users" ON users
    FOR DELETE
    USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

-- Politique pour les livres (books)
-- Tout utilisateur authentifié peut voir les livres
CREATE POLICY "Authenticated users can view books" ON books
    FOR SELECT
    USING (auth.role() = 'authenticated');

-- Seuls les administrateurs peuvent ajouter, modifier ou supprimer des livres
CREATE POLICY "Admins can insert books" ON books
    FOR INSERT
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

CREATE POLICY "Admins can update books" ON books
    FOR UPDATE
    USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

CREATE POLICY "Admins can delete books" ON books
    FOR DELETE
    USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

-- Politique pour les emprunts (borrows)
-- Les utilisateurs peuvent voir leurs propres emprunts
CREATE POLICY "Users can view their own borrows" ON borrows
    FOR SELECT
    USING (auth.uid() = user_id);

-- Les utilisateurs peuvent créer leurs propres emprunts
CREATE POLICY "Users can create their own borrows" ON borrows
    FOR INSERT
    WITH CHECK (auth.uid() = user_id);

-- Les utilisateurs peuvent mettre à jour leurs propres emprunts
CREATE POLICY "Users can update their own borrows" ON borrows
    FOR UPDATE
    USING (auth.uid() = user_id);

-- Les administrateurs peuvent voir, créer, modifier et supprimer tous les emprunts
CREATE POLICY "Admins can view all borrows" ON borrows
    FOR SELECT
    USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

CREATE POLICY "Admins can insert borrows" ON borrows
    FOR INSERT
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

CREATE POLICY "Admins can update borrows" ON borrows
    FOR UPDATE
    USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

CREATE POLICY "Admins can delete borrows" ON borrows
    FOR DELETE
    USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

-- Politique pour les réservations (reservations)
-- Les utilisateurs peuvent voir leurs propres réservations
CREATE POLICY "Users can view their own reservations" ON reservations
    FOR SELECT
    USING (auth.uid() = user_id);

-- Les utilisateurs peuvent créer leurs propres réservations
CREATE POLICY "Users can create their own reservations" ON reservations
    FOR INSERT
    WITH CHECK (auth.uid() = user_id);

-- Les utilisateurs peuvent mettre à jour leurs propres réservations
CREATE POLICY "Users can update their own reservations" ON reservations
    FOR UPDATE
    USING (auth.uid() = user_id);

-- Les administrateurs peuvent voir, créer, modifier et supprimer toutes les réservations
CREATE POLICY "Admins can view all reservations" ON reservations
    FOR SELECT
    USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

CREATE POLICY "Admins can insert reservations" ON reservations
    FOR INSERT
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

CREATE POLICY "Admins can update reservations" ON reservations
    FOR UPDATE
    USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

CREATE POLICY "Admins can delete reservations" ON reservations
    FOR DELETE
    USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

-- Politique pour les évaluations de livres (book_ratings)
-- Tout utilisateur authentifié peut voir les évaluations
CREATE POLICY "Authenticated users can view ratings" ON book_ratings
    FOR SELECT
    USING (auth.role() = 'authenticated');

-- Les utilisateurs peuvent créer leurs propres évaluations
CREATE POLICY "Users can create their own ratings" ON book_ratings
    FOR INSERT
    WITH CHECK (auth.uid() = user_id);

-- Les utilisateurs peuvent mettre à jour leurs propres évaluations
CREATE POLICY "Users can update their own ratings" ON book_ratings
    FOR UPDATE
    USING (auth.uid() = user_id);

-- Les utilisateurs peuvent supprimer leurs propres évaluations
CREATE POLICY "Users can delete their own ratings" ON book_ratings
    FOR DELETE
    USING (auth.uid() = user_id);

-- Les administrateurs peuvent supprimer toutes les évaluations
CREATE POLICY "Admins can delete ratings" ON book_ratings
    FOR DELETE
    USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );

-- Politique pour les likes de livres (book_likes)
-- Tout utilisateur authentifié peut voir les likes
CREATE POLICY "Authenticated users can view likes" ON book_likes
    FOR SELECT
    USING (auth.role() = 'authenticated');

-- Les utilisateurs peuvent créer leurs propres likes
CREATE POLICY "Users can create their own likes" ON book_likes
    FOR INSERT
    WITH CHECK (auth.uid() = user_id);

-- Les utilisateurs peuvent supprimer leurs propres likes
CREATE POLICY "Users can delete their own likes" ON book_likes
    FOR DELETE
    USING (auth.uid() = user_id);

-- Politique pour l'historique des conversations (chat_history)
-- Les utilisateurs peuvent voir leur propre historique de conversation
CREATE POLICY "Users can view their own chat history" ON chat_history
    FOR SELECT
    USING (auth.uid() = user_id);

-- Les utilisateurs peuvent créer leurs propres entrées d'historique
CREATE POLICY "Users can create their own chat history" ON chat_history
    FOR INSERT
    WITH CHECK (auth.uid() = user_id);

-- Les administrateurs peuvent voir tout l'historique des conversations
CREATE POLICY "Admins can view all chat history" ON chat_history
    FOR SELECT
    USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = auth.uid() AND users.is_admin = TRUE
        )
    );
