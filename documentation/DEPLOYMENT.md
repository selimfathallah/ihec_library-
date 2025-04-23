# Guide de déploiement pour IHEC Library

Ce document explique comment déployer l'application IHEC Library sur différentes plateformes.

## Prérequis

- .NET SDK 6.0 ou supérieur (pour la compilation)
- Zip (pour la création des archives)
- Bash (pour exécuter le script de compilation)

## Compilation et packaging

Le projet inclut un script de compilation (`build.sh`) qui automatise le processus de compilation et de packaging pour Windows, macOS et Linux.

### Utilisation du script de compilation

1. Ouvrez un terminal et naviguez vers le dossier racine du projet
2. Rendez le script exécutable si nécessaire : `chmod +x build.sh`
3. Exécutez le script : `./build.sh`
4. Les packages compilés seront disponibles dans le dossier `dist`

### Contenu des packages

Chaque package contient :
- L'application compilée pour la plateforme cible
- La documentation complète (guide d'installation, manuel d'utilisation, etc.)
- Les fichiers de configuration nécessaires

## Installation manuelle

Si vous préférez compiler et installer l'application manuellement :

### Windows

```bash
dotnet publish src/IHECLibrary -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist/IHECLibrary-Windows
```

### macOS

```bash
dotnet publish src/IHECLibrary -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o dist/IHECLibrary-macOS
```

### Linux

```bash
dotnet publish src/IHECLibrary -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o dist/IHECLibrary-Linux
```

## Configuration après déploiement

Après avoir déployé l'application, vous devrez peut-être configurer les clés API :

1. Ouvrez le fichier `App.axaml.cs` dans le dossier source
2. Localisez les variables `supabaseUrl`, `supabaseKey` et `geminiApiKey`
3. Mettez à jour ces valeurs si nécessaire

## Déploiement en production

Pour un déploiement en production, il est recommandé de :

1. Utiliser un système de gestion des secrets pour les clés API
2. Configurer un système de journalisation centralisé
3. Mettre en place un système de mise à jour automatique
4. Configurer un système de sauvegarde pour la base de données Supabase

## Résolution des problèmes de déploiement

### L'application ne démarre pas

- Vérifiez que .NET Runtime est installé sur la machine cible
- Vérifiez les journaux d'erreur dans le dossier `logs`
- Assurez-vous que les clés API sont correctement configurées

### Problèmes de connexion à Supabase

- Vérifiez que la clé API Supabase est valide
- Assurez-vous que les règles de sécurité Supabase sont correctement configurées
- Vérifiez la connectivité réseau

### Problèmes avec l'API Gemini

- Vérifiez que la clé API Gemini est valide
- Assurez-vous que l'API Gemini est disponible
- Vérifiez les quotas d'utilisation de l'API
