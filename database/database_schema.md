# Schéma de la base de données IHEC Library

## Introduction
Ce document décrit la structure de la base de données Supabase pour l'application IHEC Library. La base de données est conçue pour stocker les informations sur les utilisateurs, les livres, les emprunts, les réservations et autres données nécessaires au fonctionnement de l'application.

## Tables

### 1. Users (Utilisateurs)
Table pour stocker les informations des utilisateurs (étudiants et administrateurs).

| Colonne | Type | Description |
|---------|------|-------------|
| id | uuid | Identifiant unique, clé primaire |
| email | varchar | Email de l'utilisateur (@ihec.ucar.tn) |
| password_hash | varchar | Hash du mot de passe |
| first_name | varchar | Prénom |
| last_name | varchar | Nom de famille |
| phone_number | varchar | Numéro de téléphone (format tunisien: +216) |
| level_of_study | varchar | Niveau d'étude (1, 2, 3, M1, M2, Autre) |
| field_of_study | varchar | Domaine d'étude (BI, Gestion, Finance, etc.) |
| profile_picture_url | varchar | URL de la photo de profil |
| is_admin | boolean | Indique si l'utilisateur est un administrateur |
| job_title | varchar | Titre du poste (pour les administrateurs) |
| created_at | timestamp | Date de création du compte |
| updated_at | timestamp | Date de dernière mise à jour |
| is_approved | boolean | Indique si le compte administrateur est approuvé |
| is_blocked | boolean | Indique si l'utilisateur est bloqué |

### 2. Books (Livres)
Table pour stocker les informations sur les livres disponibles dans la bibliothèque.

| Colonne | Type | Description |
|---------|------|-------------|
| id | uuid | Identifiant unique, clé primaire |
| title | varchar | Titre du livre |
| author | varchar | Auteur du livre |
| isbn | varchar | Numéro ISBN |
| publication_year | integer | Année de publication |
| publisher | varchar | Éditeur |
| category | varchar | Catégorie du livre |
| description | text | Description du livre |
| cover_image_url | varchar | URL de l'image de couverture |
| available_copies | integer | Nombre d'exemplaires disponibles |
| total_copies | integer | Nombre total d'exemplaires |
| created_at | timestamp | Date d'ajout à la bibliothèque |
| updated_at | timestamp | Date de dernière mise à jour |

### 3. Borrows (Emprunts)
Table pour enregistrer les emprunts de livres par les utilisateurs.

| Colonne | Type | Description |
|---------|------|-------------|
| id | uuid | Identifiant unique, clé primaire |
| user_id | uuid | ID de l'utilisateur (clé étrangère) |
| book_id | uuid | ID du livre (clé étrangère) |
| borrow_date | timestamp | Date d'emprunt |
| due_date | timestamp | Date de retour prévue |
| return_date | timestamp | Date de retour effective (null si non retourné) |
| is_returned | boolean | Indique si le livre a été retourné |
| created_at | timestamp | Date de création de l'enregistrement |
| updated_at | timestamp | Date de dernière mise à jour |

### 4. Reservations (Réservations)
Table pour enregistrer les réservations de livres par les utilisateurs.

| Colonne | Type | Description |
|---------|------|-------------|
| id | uuid | Identifiant unique, clé primaire |
| user_id | uuid | ID de l'utilisateur (clé étrangère) |
| book_id | uuid | ID du livre (clé étrangère) |
| reservation_date | timestamp | Date de réservation |
| is_active | boolean | Indique si la réservation est active |
| is_notified | boolean | Indique si l'utilisateur a été notifié de la disponibilité |
| created_at | timestamp | Date de création de l'enregistrement |
| updated_at | timestamp | Date de dernière mise à jour |

### 5. BookRatings (Évaluations de livres)
Table pour stocker les évaluations et commentaires des utilisateurs sur les livres.

| Colonne | Type | Description |
|---------|------|-------------|
| id | uuid | Identifiant unique, clé primaire |
| user_id | uuid | ID de l'utilisateur (clé étrangère) |
| book_id | uuid | ID du livre (clé étrangère) |
| rating | integer | Note (1-5) |
| comment | text | Commentaire |
| created_at | timestamp | Date de création de l'évaluation |
| updated_at | timestamp | Date de dernière mise à jour |

### 6. BookLikes (Livres aimés)
Table pour enregistrer les livres aimés par les utilisateurs.

| Colonne | Type | Description |
|---------|------|-------------|
| id | uuid | Identifiant unique, clé primaire |
| user_id | uuid | ID de l'utilisateur (clé étrangère) |
| book_id | uuid | ID du livre (clé étrangère) |
| created_at | timestamp | Date de création de l'enregistrement |

### 7. ChatHistory (Historique des conversations)
Table pour stocker l'historique des conversations avec le chatbot HEC 1.0.

| Colonne | Type | Description |
|---------|------|-------------|
| id | uuid | Identifiant unique, clé primaire |
| user_id | uuid | ID de l'utilisateur (clé étrangère) |
| message | text | Message de l'utilisateur |
| response | text | Réponse du chatbot |
| created_at | timestamp | Date de création de l'enregistrement |

## Relations

1. Users (1) -> (*) Borrows : Un utilisateur peut emprunter plusieurs livres
2. Users (1) -> (*) Reservations : Un utilisateur peut réserver plusieurs livres
3. Users (1) -> (*) BookRatings : Un utilisateur peut évaluer plusieurs livres
4. Users (1) -> (*) BookLikes : Un utilisateur peut aimer plusieurs livres
5. Users (1) -> (*) ChatHistory : Un utilisateur peut avoir plusieurs conversations avec le chatbot
6. Books (1) -> (*) Borrows : Un livre peut être emprunté plusieurs fois (par différents utilisateurs)
7. Books (1) -> (*) Reservations : Un livre peut être réservé plusieurs fois (par différents utilisateurs)
8. Books (1) -> (*) BookRatings : Un livre peut avoir plusieurs évaluations
9. Books (1) -> (*) BookLikes : Un livre peut être aimé par plusieurs utilisateurs

## Règles de sécurité et d'accès

1. Les utilisateurs non authentifiés ne peuvent pas accéder aux données
2. Les utilisateurs authentifiés peuvent :
   - Lire les informations sur tous les livres
   - Lire les évaluations et commentaires publics
   - Gérer leurs propres emprunts, réservations, évaluations et likes
   - Voir leur propre historique de conversation avec le chatbot
3. Les administrateurs peuvent :
   - Lire et modifier toutes les données
   - Ajouter, modifier et supprimer des livres
   - Gérer les comptes utilisateurs (approuver, bloquer)
   - Voir les statistiques d'utilisation

## Fonctions et déclencheurs

1. Déclencheur pour mettre à jour le nombre d'exemplaires disponibles lorsqu'un livre est emprunté ou retourné
2. Déclencheur pour envoyer une notification par email lorsqu'un livre réservé devient disponible
3. Fonction pour calculer le classement des utilisateurs (Bronze, Silver, Gold, Master) en fonction du nombre de livres empruntés
4. Fonction pour générer des recommandations de livres basées sur les préférences de l'utilisateur
