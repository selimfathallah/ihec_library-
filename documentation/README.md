# Documentation de l'Application IHEC Library

## Table des matières
1. [Introduction](#introduction)
2. [Guide d'installation](#guide-dinstallation)
3. [Manuel d'utilisation](#manuel-dutilisation)
4. [Documentation technique](#documentation-technique)
5. [Guide du développeur](#guide-du-développeur)

## Introduction

IHEC Library est une application de bureau conçue spécifiquement pour la communauté du collège "IHEC Carthage". Cette application permet aux étudiants et au personnel administratif de gérer et d'accéder aux ressources académiques de la bibliothèque de manière efficace et intuitive.

### Fonctionnalités principales

- **Authentification sécurisée** : Système de connexion et d'inscription pour les étudiants et les administrateurs
- **Catalogue de livres** : Recherche, filtrage et consultation des livres disponibles
- **Gestion des emprunts** : Emprunt, réservation et retour de livres
- **Profil utilisateur** : Suivi des emprunts, réservations et recommandations personnalisées
- **Chatbot HEC 1.0** : Assistant virtuel alimenté par l'IA Gemini pour répondre aux questions des utilisateurs
- **Tableau de bord administratif** : Gestion complète de la bibliothèque pour les administrateurs

### Technologies utilisées

- **Frontend** : Avalonia UI (.NET 6.0)
- **Backend** : Supabase (base de données PostgreSQL)
- **IA** : API Gemini de Google
- **Architecture** : MVVM (Model-View-ViewModel)

## Guide d'installation

### Prérequis système

- Système d'exploitation : Windows 10/11, macOS 10.15+, ou Linux (Ubuntu 20.04+)
- .NET 6.0 Runtime ou supérieur
- Connexion Internet (pour l'accès à Supabase et à l'API Gemini)
- Minimum 2 Go de RAM
- Minimum 100 Mo d'espace disque disponible

### Installation sur Windows

1. Téléchargez le fichier d'installation `IHECLibrary-Setup.exe` depuis le dossier de distribution
2. Exécutez le fichier d'installation et suivez les instructions à l'écran
3. Une fois l'installation terminée, lancez l'application depuis le menu Démarrer ou le raccourci sur le bureau

### Installation sur macOS

1. Téléchargez le fichier `IHECLibrary.dmg` depuis le dossier de distribution
2. Ouvrez le fichier DMG et faites glisser l'application dans le dossier Applications
3. Lancez l'application depuis le Launchpad ou le dossier Applications

### Installation sur Linux

1. Téléchargez le fichier `IHECLibrary.AppImage` depuis le dossier de distribution
2. Rendez le fichier exécutable avec la commande : `chmod +x IHECLibrary.AppImage`
3. Exécutez l'application avec la commande : `./IHECLibrary.AppImage`

### Installation depuis les sources

Si vous souhaitez installer l'application à partir des sources :

1. Assurez-vous d'avoir installé .NET SDK 6.0 ou supérieur
2. Clonez le dépôt ou téléchargez les fichiers source
3. Ouvrez un terminal dans le dossier du projet
4. Exécutez la commande : `dotnet build --configuration Release`
5. Naviguez vers le dossier de sortie et lancez l'application avec : `dotnet IHECLibrary.dll`

## Manuel d'utilisation

### Première utilisation

Lors de la première utilisation de l'application, vous serez accueilli par l'écran de connexion. Si vous n'avez pas encore de compte, vous pouvez en créer un en cliquant sur le bouton "S'inscrire".

#### Inscription

1. Cliquez sur "S'inscrire" depuis l'écran de connexion
2. Remplissez le formulaire avec vos informations personnelles :
   - Adresse e-mail (utilisez votre e-mail académique @ihec.ucar.tn)
   - Mot de passe (minimum 8 caractères, incluant au moins une majuscule, un chiffre et un caractère spécial)
   - Prénom et nom
   - Numéro de téléphone
   - Niveau d'études
   - Domaine d'études
3. Cliquez sur "Créer un compte"
4. Vous serez automatiquement connecté après l'inscription

#### Connexion

1. Entrez votre adresse e-mail et votre mot de passe
2. Cliquez sur "Se connecter"
3. Vous pouvez également vous connecter avec votre compte Google en cliquant sur le bouton correspondant

### Navigation dans l'application

L'application est divisée en quatre sections principales accessibles depuis la barre de navigation en haut de l'écran :

- **Accueil** : Recommandations personnalisées et nouveautés
- **Bibliothèque** : Catalogue complet des livres
- **HEC 1.0** : Chatbot intelligent
- **Profil** : Informations personnelles et activités

### Recherche et emprunt de livres

#### Recherche de livres

1. Accédez à la section "Bibliothèque"
2. Utilisez la barre de recherche en haut pour trouver des livres par titre, auteur ou mots-clés
3. Utilisez les filtres sur le côté gauche pour affiner votre recherche par catégorie, disponibilité ou langue

#### Consultation des détails d'un livre

1. Cliquez sur un livre dans la liste pour afficher ses détails
2. Sur la page de détails, vous pouvez voir :
   - Le titre, l'auteur et la couverture du livre
   - La description et les informations de publication
   - La disponibilité (nombre d'exemplaires disponibles)
   - Les évaluations et commentaires des autres utilisateurs

#### Emprunt d'un livre

1. Sur la page de détails d'un livre, cliquez sur le bouton "Emprunter"
2. Confirmez votre demande d'emprunt
3. Le livre sera ajouté à votre liste d'emprunts dans votre profil
4. La date de retour prévue sera affichée

#### Réservation d'un livre

Si tous les exemplaires d'un livre sont actuellement empruntés :

1. Sur la page de détails du livre, cliquez sur le bouton "Réserver"
2. Confirmez votre demande de réservation
3. Vous serez notifié lorsqu'un exemplaire sera disponible

### Utilisation du chatbot HEC 1.0

1. Accédez à la section "HEC 1.0"
2. Posez vos questions dans la zone de texte en bas de l'écran
3. Le chatbot peut vous aider avec :
   - Des recommandations de livres
   - Des informations sur la bibliothèque (horaires, règles, etc.)
   - De l'aide pour vos recherches académiques
   - Des réponses à vos questions générales sur les études

### Gestion de votre profil

1. Accédez à la section "Profil"
2. Consultez vos statistiques :
   - Livres actuellement empruntés
   - Livres réservés
   - Livres aimés
   - Votre rang de lecteur
3. Pour modifier vos informations personnelles, cliquez sur "Modifier le profil"

### Fonctionnalités administratives

Si vous avez un compte administrateur :

1. Connectez-vous avec vos identifiants administrateur
2. Vous aurez accès au tableau de bord administratif
3. Depuis ce tableau de bord, vous pouvez :
   - Gérer le catalogue de livres (ajouter, modifier, supprimer)
   - Gérer les utilisateurs
   - Suivre les emprunts et réservations
   - Approuver les demandes d'inscription administrateur

## Documentation technique

### Architecture de l'application

L'application IHEC Library est construite selon l'architecture MVVM (Model-View-ViewModel) qui sépare clairement l'interface utilisateur, la logique métier et les données.

#### Structure des dossiers

```
IHECLibrary/
├── Models/             # Modèles de données
├── ViewModels/         # ViewModels pour la logique de présentation
├── Views/              # Vues XAML pour l'interface utilisateur
├── Services/           # Services pour la logique métier
│   ├── Interfaces/     # Interfaces des services
│   └── Implementations/# Implémentations concrètes des services
├── Tests/              # Tests unitaires et fonctionnels
└── Assets/             # Ressources (images, icônes, etc.)
```

### Modèles de données

Les principaux modèles de données incluent :

- **UserModel** : Représente un utilisateur de l'application
- **BookModel** : Représente un livre dans la bibliothèque
- **BorrowingModel** : Représente un emprunt de livre
- **ReservationModel** : Représente une réservation de livre
- **AdminModel** : Représente un administrateur

### Services

L'application utilise plusieurs services pour interagir avec les APIs externes et gérer la logique métier :

- **IAuthService** : Gestion de l'authentification
- **IUserService** : Gestion des profils utilisateurs
- **IBookService** : Gestion des livres et des emprunts
- **IChatbotService** : Intégration avec l'API Gemini
- **IAdminService** : Fonctionnalités d'administration
- **INavigationService** : Navigation entre les vues

### Intégration avec Supabase

L'application utilise Supabase comme backend pour stocker et gérer les données. La structure de la base de données comprend les tables suivantes :

- **profiles** : Informations des utilisateurs
- **admin_profiles** : Informations des administrateurs
- **books** : Catalogue de livres
- **borrowings** : Enregistrements des emprunts
- **reservations** : Enregistrements des réservations
- **book_ratings** : Évaluations des livres
- **book_likes** : "J'aime" sur les livres
- **activities** : Journal des activités

### Intégration avec l'API Gemini

Le chatbot HEC 1.0 utilise l'API Gemini de Google pour générer des réponses intelligentes aux questions des utilisateurs. L'intégration est gérée par le service `GeminiChatbotService` qui envoie les requêtes à l'API et traite les réponses.

### Système de journalisation

L'application utilise Serilog pour la journalisation des événements, des erreurs et des informations de débogage. Les journaux sont stockés dans le dossier `logs` de l'application.

## Guide du développeur

### Configuration de l'environnement de développement

1. Installez Visual Studio 2022 ou JetBrains Rider
2. Installez .NET SDK 6.0 ou supérieur
3. Installez les extensions Avalonia pour votre IDE
4. Clonez le dépôt du projet

### Compilation du projet

```bash
# Restaurer les packages NuGet
dotnet restore

# Compiler le projet
dotnet build

# Exécuter l'application
dotnet run --project src/IHECLibrary/IHECLibrary.csproj
```

### Exécution des tests

```bash
# Exécuter tous les tests
dotnet test

# Exécuter les tests avec l'application
dotnet run --project src/IHECLibrary/IHECLibrary.csproj -- --test
```

### Ajout de nouvelles fonctionnalités

Pour ajouter une nouvelle fonctionnalité à l'application :

1. Créez ou modifiez les modèles de données nécessaires
2. Implémentez les services requis
3. Créez le ViewModel correspondant
4. Créez la vue XAML
5. Mettez à jour le service de navigation
6. Ajoutez des tests pour la nouvelle fonctionnalité

### Déploiement

Pour créer un package de déploiement :

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained true

# macOS
dotnet publish -c Release -r osx-x64 --self-contained true

# Linux
dotnet publish -c Release -r linux-x64 --self-contained true
```

### Modification des clés API

Les clés API pour Supabase et Gemini sont configurées dans la classe `App.axaml.cs`. Pour les modifier :

1. Ouvrez le fichier `src/IHECLibrary/App.axaml.cs`
2. Localisez la méthode `ConfigureServices`
3. Mettez à jour les valeurs des variables `supabaseUrl`, `supabaseKey` et `geminiApiKey`

### Contribution au projet

Si vous souhaitez contribuer au projet :

1. Créez une branche pour votre fonctionnalité ou correction
2. Implémentez vos modifications
3. Assurez-vous que tous les tests passent
4. Soumettez une pull request avec une description détaillée de vos modifications
