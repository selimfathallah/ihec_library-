# Guide d'installation de l'application IHEC Library

Ce guide vous explique comment installer et configurer l'application IHEC Library sur votre ordinateur.

## Prérequis système

Avant de commencer l'installation, assurez-vous que votre système répond aux exigences minimales suivantes :

- **Système d'exploitation** : Windows 10/11, macOS 10.15+, ou Linux (Ubuntu 20.04+)
- **.NET Runtime** : .NET 6.0 ou supérieur
- **Mémoire RAM** : Minimum 2 Go
- **Espace disque** : Minimum 100 Mo d'espace disponible
- **Connexion Internet** : Requise pour l'accès à Supabase et à l'API Gemini

## Installation du .NET Runtime

Si vous n'avez pas déjà installé .NET 6.0 Runtime sur votre ordinateur, suivez ces étapes :

### Windows

1. Téléchargez le programme d'installation de .NET 6.0 Runtime depuis le [site officiel de Microsoft](https://dotnet.microsoft.com/download/dotnet/6.0)
2. Exécutez le programme d'installation et suivez les instructions à l'écran
3. Redémarrez votre ordinateur si nécessaire

### macOS

1. Téléchargez le programme d'installation de .NET 6.0 Runtime pour macOS depuis le [site officiel de Microsoft](https://dotnet.microsoft.com/download/dotnet/6.0)
2. Ouvrez le fichier PKG téléchargé et suivez les instructions d'installation
3. Redémarrez votre ordinateur si nécessaire

### Linux (Ubuntu)

Exécutez les commandes suivantes dans un terminal :

```bash
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install -y apt-transport-https
sudo apt-get update
sudo apt-get install -y dotnet-runtime-6.0
```

## Installation de l'application IHEC Library

### Installation à partir du package précompilé

#### Windows

1. Téléchargez le fichier `IHECLibrary-Setup.exe` depuis le dossier de distribution
2. Double-cliquez sur le fichier d'installation
3. Suivez les instructions à l'écran pour compléter l'installation
4. Une fois l'installation terminée, l'application peut être lancée depuis le menu Démarrer ou le raccourci sur le bureau

#### macOS

1. Téléchargez le fichier `IHECLibrary.dmg` depuis le dossier de distribution
2. Double-cliquez sur le fichier DMG pour le monter
3. Faites glisser l'application IHEC Library dans votre dossier Applications
4. Lancez l'application depuis le Launchpad ou le dossier Applications

#### Linux

1. Téléchargez le fichier `IHECLibrary.AppImage` depuis le dossier de distribution
2. Ouvrez un terminal et naviguez vers le dossier contenant le fichier téléchargé
3. Rendez le fichier exécutable avec la commande : `chmod +x IHECLibrary.AppImage`
4. Exécutez l'application avec la commande : `./IHECLibrary.AppImage`

### Installation à partir des sources

Si vous préférez installer l'application à partir des sources :

1. Assurez-vous d'avoir installé .NET SDK 6.0 ou supérieur (pas seulement le Runtime)
2. Téléchargez ou clonez le dépôt du projet
3. Ouvrez un terminal et naviguez vers le dossier racine du projet
4. Exécutez les commandes suivantes :

```bash
dotnet restore
dotnet build --configuration Release
```

5. Pour lancer l'application, exécutez :

```bash
dotnet run --project src/IHECLibrary/IHECLibrary.csproj
```

## Configuration initiale

Lors du premier lancement de l'application, vous devrez vous connecter ou créer un compte :

1. Pour créer un nouveau compte, cliquez sur "S'inscrire" sur l'écran de connexion
2. Remplissez le formulaire avec vos informations personnelles
   - Utilisez votre adresse e-mail académique (@ihec.ucar.tn)
   - Choisissez un mot de passe sécurisé (minimum 8 caractères)
   - Remplissez tous les champs obligatoires
3. Cliquez sur "Créer un compte" pour finaliser l'inscription
4. Vous serez automatiquement connecté à l'application

## Résolution des problèmes courants

### L'application ne démarre pas

- Vérifiez que vous avez installé .NET 6.0 Runtime ou supérieur
- Vérifiez les journaux d'erreur dans le dossier `logs` de l'application
- Essayez de réinstaller l'application

### Problèmes de connexion

- Vérifiez votre connexion Internet
- Assurez-vous que vous utilisez les bons identifiants
- Si vous avez oublié votre mot de passe, utilisez l'option "Mot de passe oublié" sur l'écran de connexion

### Erreurs lors de l'utilisation

- Consultez les journaux d'erreur dans le dossier `logs` de l'application
- Redémarrez l'application
- Si le problème persiste, contactez l'administrateur système

## Support technique

Pour toute assistance technique, veuillez contacter :

- **Email** : support.bibliotheque@ihec.ucar.tn
- **Téléphone** : +216 71 775 948
- **Bureau** : Service informatique, 1er étage, salle 105
